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
}

function parseReferences(text: string): ParsedRef[] {
  const refs: ParsedRef[] = [];
  // Match patterns like [Source: Title, Page X, Section Y] or [Source: Title, Timestamp HH:MM:SS]
  const regex = /\[Source:\s*([^,\]]+?)(?:,\s*Page\s*(\d+))?(?:,\s*Section\s*(\d+|[^,\]]+))?(?:,\s*Timestamp\s*([\d:]+))?\s*\]​?/gi;
  let match;
  while ((match = regex.exec(text)) !== null) {
    refs.push({
      fullMatch: match[0],
      sourceTitle: match[1].trim(),
      page: match[2] ? parseInt(match[2], 10) : undefined,
      section: match[3] ? (isNaN(Number(match[3])) ? undefined : parseInt(match[3], 10)) : undefined,
      timestamp: match[4] || undefined,
    });
  }
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

  const parts: (string | JSX.Element)[] = [];
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

    if (material) {
      parts.push(
        <button
          key={`ref-${idx}`}
          onClick={() => {
            const ts = ref.timestamp ? timestampToSeconds(ref.timestamp) : undefined;
            onOpenMaterial(material.id, ref.page, ts);
          }}
          className="inline-flex items-center gap-1 px-1.5 py-0.5 mx-0.5 text-xs font-medium rounded-md bg-primary/10 text-primary hover:bg-primary/20 transition-colors border border-primary/20 cursor-pointer"
          title={`Open ${ref.sourceTitle}${ref.page ? ` at page ${ref.page}` : ''}${ref.timestamp ? ` at ${ref.timestamp}` : ''}`}
        >
          {isVideo ? <Video className="h-3 w-3" /> : <FileText className="h-3 w-3" />}
          <span>{ref.sourceTitle}</span>
          {ref.page && <span className="opacity-70">p.{ref.page}</span>}
          {ref.timestamp && <span className="opacity-70">{ref.timestamp}</span>}
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
