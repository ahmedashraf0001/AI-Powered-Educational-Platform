import type { ReactNode } from 'react';
import { ExternalLink, FileText, Video } from 'lucide-react';

export interface MaterialInfo {
  id: string;
  title: string;
  materialType?: string;
}

interface SourceReferenceProps {
  text: string;
  materials: MaterialInfo[];
  onOpenMaterial: (materialId: string, page?: number, timestamp?: number) => void;
}

interface ParsedRef {
  fullMatch: string;
  sourceTitle: string;
  page?: number;
  section?: number;
  timestamp?: string;
  timestampEnd?: string;
}

function parseReferences(text: string): ParsedRef[] {
  const refs: ParsedRef[] = [];

  // Pattern 1: [Source: ...] with optional locator tokens after commas.
  // Examples:
  // - [Source: chapter1.pdf, p. 12]
  // - [Source: videoplayback.mp4, Timestamp 00:00:10]
  // - [Source: videoplayback.mp4, 00:00:10-00:00:15]
  // - [Source: videoplayback.mp4, 00:00:10-00:00:15].
  const regex1 = /\[Source:\s*([^\]]+?)\s*\]\u200B?/gi;

  // Pattern 2: Simple format - "Title\np.X" or "Title p.X" at end of text
  const regex2 = /([A-Za-z0-9][^.\n]*\.(?:pdf|doc|docx|ppt|pptx|mp4|mp3|wav))\s*\n?\s*(?:p\.?|page)\s*(\d+)/gi;

  let match: RegExpExecArray | null;

  // Find Pattern 1 matches
  while ((match = regex1.exec(text)) !== null) {
    const raw = match[1].trim();
    const parts = raw
      .split(',')
      .map((part) => part.trim())
      .filter(Boolean);

    if (parts.length === 0) continue;

    let page: number | undefined;
    let section: number | undefined;
    let timestamp: string | undefined;
    let timestampEnd: string | undefined;

    // First token is source title, remaining tokens are optional locators.
    const sourceTitle = parts[0];

    for (const locatorRaw of parts.slice(1)) {
      const pageMatch = locatorRaw.match(/^(?:Page|p\.?)\s*(\d+)$/i);
      if (pageMatch) {
        page = parseInt(pageMatch[1], 10);
        continue;
      }

      const sectionMatch = locatorRaw.match(/^(?:Section|s\.?)\s*(.+)$/i);
      if (sectionMatch) {
        const sectionValue = Number(sectionMatch[1]);
        section = Number.isNaN(sectionValue) ? undefined : parseInt(sectionMatch[1], 10);
        continue;
      }

      // Support either labeled or unlabeled timestamp values.
      const labeledTimestamp = locatorRaw.match(/^(?:Timestamp|t\.?|@)\s*(.+)$/i);
      const timestampCandidate = (labeledTimestamp ? labeledTimestamp[1] : locatorRaw).trim();

      const rangeMatch = timestampCandidate.match(
        /^(\d{1,2}:\d{2}(?::\d{2})?)\s*(?:-|\u2012|\u2013|\u2014)\s*(\d{1,2}:\d{2}(?::\d{2})?)$/
      );
      if (rangeMatch) {
        timestamp = rangeMatch[1];
        timestampEnd = rangeMatch[2];
        continue;
      }

      const pointMatch = timestampCandidate.match(/^(\d{1,2}:\d{2}(?::\d{2})?)$/);
      if (pointMatch) {
        timestamp = pointMatch[1];
      }
    }

    refs.push({
      fullMatch: match[0],
      sourceTitle,
      page,
      section,
      timestamp,
      timestampEnd,
    });
  }

  // Find Pattern 2 matches (only if not already matched by Pattern 1)
  while ((match = regex2.exec(text)) !== null) {
    const matchIndex = match.index;
    const isDuplicate = refs.some(r =>
      text.indexOf(r.fullMatch) <= matchIndex &&
      matchIndex < text.indexOf(r.fullMatch) + r.fullMatch.length
    );
    if (!isDuplicate) {
      refs.push({
        fullMatch: match[0],
        sourceTitle: match[1].trim(),
        page: match[2] ? parseInt(match[2], 10) : undefined,
      });
    }
  }

  // Sort by position in text
  refs.sort((a, b) => text.indexOf(a.fullMatch) - text.indexOf(b.fullMatch));

  return refs;
}

function timestampToSeconds(ts: string): number {
  const parts = ts.split(':').map(Number);
  if (parts.length === 3) return parts[0] * 3600 + parts[1] * 60 + parts[2];
  if (parts.length === 2) return parts[0] * 60 + parts[1];
  return parts[0] || 0;
}

function findMaterial(materials: MaterialInfo[], sourceTitle: string): MaterialInfo | undefined {
  const normalized = sourceTitle.toLowerCase().replace(/\s+/g, ' ').trim();
  return materials.find((m) => {
    const mTitle = m.title.toLowerCase().replace(/\s+/g, ' ').trim();
    return mTitle === normalized || mTitle.includes(normalized) || normalized.includes(mTitle);
  });
}

/**
 * Renders text with clickable source references.
 * Parses [Source: ...] patterns and replaces them with buttons.
 */
export function SourceReference({ text, materials, onOpenMaterial }: SourceReferenceProps) {
  const refs = parseReferences(text);
  if (refs.length === 0) return <>{text}</>;

  const parts: (string | ReactNode)[] = [];
  let lastIdx = 0;

  refs.forEach((ref, idx) => {
    const refStart = text.indexOf(ref.fullMatch, lastIdx);
    if (refStart === -1) return;

    // Add text before this reference
    if (refStart > lastIdx) {
      parts.push(text.substring(lastIdx, refStart));
    }

    const material = findMaterial(materials, ref.sourceTitle);
    const isVideo = material?.materialType?.toLowerCase() === 'video' || material?.materialType?.toLowerCase() === 'audio';
    const timestampLabel = ref.timestampEnd
      ? `${ref.timestamp}-${ref.timestampEnd}`
      : ref.timestamp;

    if (material) {
      parts.push(
        <button
          key={`ref-${idx}`}
          onClick={() => {
            const ts = ref.timestamp ? timestampToSeconds(ref.timestamp) : undefined;
            onOpenMaterial(material.id, ref.page, ts);
          }}
          className="inline-flex items-center gap-1 px-1.5 py-0.5 mx-0.5 text-xs font-medium rounded-md bg-primary/10 text-primary hover:bg-primary/20 transition-colors border border-primary/20 cursor-pointer"
          title={`Open ${ref.sourceTitle}${ref.page ? ` at page ${ref.page}` : ''}${timestampLabel ? ` at ${timestampLabel}` : ''}`}
        >
          {isVideo ? <Video className="h-3 w-3" /> : <FileText className="h-3 w-3" />}
          <span>{ref.sourceTitle}</span>
          {ref.page && <span className="opacity-70">p.{ref.page}</span>}
          {timestampLabel && <span className="opacity-70">{timestampLabel}</span>}
          <ExternalLink className="h-2.5 w-2.5 opacity-50" />
        </button>
      );
    } else {
      // Material not found — render as plain styled badge
      parts.push(
        <span
          key={`ref-${idx}`}
          className="inline-flex items-center gap-1 px-1.5 py-0.5 mx-0.5 text-xs font-medium rounded-md bg-secondary text-muted-foreground border"
        >
          <FileText className="h-3 w-3" />
          {ref.sourceTitle}
          {ref.page && <span className="opacity-70">p.{ref.page}</span>}
          {timestampLabel && <span className="opacity-70">{timestampLabel}</span>}
        </span>
      );
    }

    lastIdx = refStart + ref.fullMatch.length;
  });

  // Add remaining text
  if (lastIdx < text.length) {
    parts.push(text.substring(lastIdx));
  }

  return <>{parts}</>;
}

/**
 * Custom ReactMarkdown renderer that makes [Source: ...] references clickable.
 * Use as a wrapper for text nodes inside markdown.
 */
export function renderTextWithRefs(
  text: string,
  materials: MaterialInfo[],
  onOpenMaterial: (materialId: string, page?: number, timestamp?: number) => void
) {
  const refs = parseReferences(text);
  if (refs.length === 0) return text;

  return (
    <SourceReference text={text} materials={materials} onOpenMaterial={onOpenMaterial} />
  );
}
