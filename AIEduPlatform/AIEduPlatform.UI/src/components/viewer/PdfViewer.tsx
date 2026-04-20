import { useState, useCallback, useEffect, useRef } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';
import { Button } from '@/components/ui/Button';
import { Spinner } from '@/components/ui/Spinner';
import {
  ChevronLeft,
  ChevronRight,
  ZoomIn,
  ZoomOut,
  Download,
  Maximize2,
  Minimize2,
  FileQuestion,
  BookOpen,
  Lightbulb,
  ChevronDown,
  List,
} from 'lucide-react';

// Set up PDF.js worker
pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;

import 'react-pdf/dist/Page/AnnotationLayer.css';
import 'react-pdf/dist/Page/TextLayer.css';

interface Section {
  id: string;
  title: string;
  summary: string;
  startPage: number | null;
  endPage: number | null;
  orderIndex: number;
}

interface PdfViewerProps {
  url: string;
  sections: Section[];
  initialPage?: number;
  scrollTrigger?: number;
  onDownload?: () => void;
  onSectionAction?: (type: 'quiz' | 'summary' | 'flashcards', sectionId: string) => void;
  isFullscreen?: boolean;
  onToggleFullscreen?: () => void;
  loadingSection?: { type: string; sectionId: string } | null;
  onPageChange?: (pageNumber: number) => void;
}

const ZOOM_LEVELS = [0.5, 0.75, 1, 1.25, 1.5, 2];

export function PdfViewer({
  url,
  sections,
  initialPage = 1,
  scrollTrigger = 0,
  onDownload,
  onSectionAction,
  isFullscreen = false,
  onToggleFullscreen,
  loadingSection,
  onPageChange,
}: PdfViewerProps) {
  const [numPages, setNumPages] = useState<number>(0);
  const [pageNumber, setPageNumber] = useState(initialPage);
  const [scale, setScale] = useState(1);
  const [showSidebar, setShowSidebar] = useState(true);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const handleDocumentLoadSuccess = useCallback(({ numPages }: { numPages: number }) => {
    setNumPages(numPages);
    setLoading(false);
    setError(null);
  }, []);

  const handleDocumentLoadError = useCallback((err: Error) => {
    console.error('PDF load error:', err);
    setError('Failed to load PDF');
    setLoading(false);
  }, []);

  const goToPage = useCallback((page: number) => {
    const newPage = Math.max(1, Math.min(page, numPages || page));
    setPageNumber((prev) => {
      if (prev !== newPage) {
        if (onPageChange) {
          onPageChange(newPage);
        }
        return newPage;
      }
      return prev;
    });
    
    // Scroll to page element
    setTimeout(() => {
      const element = document.getElementById(`pdf-page-${newPage}`);
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 100);
  }, [numPages, onPageChange]);

  // Handle initialization and external scroll triggers
  const lastScrollTrigger = useRef(scrollTrigger);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if (numPages > 0) {
      if (!hasInitialized.current && initialPage) {
        goToPage(initialPage);
        hasInitialized.current = true;
      } else if (scrollTrigger !== lastScrollTrigger.current && initialPage) {
        goToPage(initialPage);
        lastScrollTrigger.current = scrollTrigger;
      }
    }
  }, [initialPage, numPages, scrollTrigger, goToPage]);

  const zoomIn = useCallback(() => {
    const currentIndex = ZOOM_LEVELS.indexOf(scale);
    if (currentIndex < ZOOM_LEVELS.length - 1) {
      setScale(ZOOM_LEVELS[currentIndex + 1]);
    }
  }, [scale]);

  const zoomOut = useCallback(() => {
    const currentIndex = ZOOM_LEVELS.indexOf(scale);
    if (currentIndex > 0) {
      setScale(ZOOM_LEVELS[currentIndex - 1]);
    }
  }, [scale]);

  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    const container = e.currentTarget;
    const scrollPosition = container.scrollTop;
    
    // Find the page currently in view based on scroll position
    let closestPage = 1;
    let minDistance = Infinity;

    for (let i = 1; i <= numPages; i++) {
        const element = document.getElementById(`pdf-page-${i}`);
        if (element) {
            const distance = Math.abs(element.offsetTop - scrollPosition - container.offsetTop);
            if (distance < minDistance) {
                minDistance = distance;
                closestPage = i;
            }
        }
    }
    
    setPageNumber((prev) => {
      if (closestPage !== prev) {
        if (onPageChange) {
          onPageChange(closestPage);
        }
        return closestPage;
      }
      return prev;
    });
  }, [numPages, onPageChange]);

  // Find current section based on page
  const currentSection = sections.find((s) => {
    const start = s.startPage ?? 1;
    const end = s.endPage ?? numPages;
    return pageNumber >= start && pageNumber <= end;
  });

  // Sort sections by orderIndex or startPage
  const sortedSections = [...sections].sort((a, b) => {
    if (a.orderIndex !== b.orderIndex) return a.orderIndex - b.orderIndex;
    return (a.startPage ?? 0) - (b.startPage ?? 0);
  });

  return (
    <div className={`flex flex-col h-full ${isFullscreen ? 'bg-background' : ''}`}>
      {/* Toolbar */}
      <div className="flex items-center justify-between gap-4 p-4 border-b bg-secondary/30 flex-wrap shadow-sm rounded-t-sm">
        <div className="flex items-center gap-3">
          {/* Toggle sidebar */}
          <Button
            variant="ghost"
            size="sm"
            className="rounded-full hover:bg-secondary"
            onClick={() => setShowSidebar(!showSidebar)}
            title={showSidebar ? 'Hide sections' : 'Show sections'}
          >
            <List className="h-4 w-4" />
          </Button>

          {/* Page navigation */}
          <div className="flex items-center gap-1.5 ml-2">
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8 rounded-full bg-background"
              onClick={() => goToPage(pageNumber - 1)}
              disabled={pageNumber <= 1}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <div className="flex items-center gap-2 text-sm font-medium px-2">
              <input
                type="number"
                value={pageNumber}
                onChange={(e) => goToPage(parseInt(e.target.value) || 1)}
                className="w-12 h-7 text-center border rounded bg-background text-sm"
                min={1}
                max={numPages}
              />
<span className="text-muted-foreground mr-1">/ {numPages}</span>
            </div>
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8 rounded-full bg-background"
              onClick={() => goToPage(pageNumber + 1)}
              disabled={pageNumber >= numPages}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {/* Current section indicator */}
          {currentSection && (
            <span className="text-xs font-semibold text-primary/80 bg-primary/10 px-3 py-1 rounded-full mr-2 max-w-[200px] truncate hidden sm:inline">
              {currentSection.title}
            </span>
          )}

          {/* Zoom controls */}
          <div className="flex items-center bg-secondary/50 rounded-full p-0.5 shadow-sm border border-border/50">
            <Button variant="ghost" size="icon" className="h-8 w-8 rounded-full hover:bg-background" onClick={zoomOut} disabled={scale <= ZOOM_LEVELS[0]}>
              <ZoomOut className="h-4 w-4" />
            </Button>
            <span className="text-xs font-medium w-12 text-center select-none">{Math.round(scale * 100)}%</span>
            <Button variant="ghost" size="icon" className="h-8 w-8 rounded-full hover:bg-background" onClick={zoomIn} disabled={scale >= ZOOM_LEVELS[ZOOM_LEVELS.length - 1]}>
              <ZoomIn className="h-4 w-4" />
            </Button>
          </div>

          {/* Download */}
          {onDownload && (
            <Button variant="outline" size="sm" className="rounded-full h-8 ml-1 bg-background" onClick={onDownload}>
              <Download className="h-4 w-4 sm:mr-1.5" />
              <span className="hidden sm:inline">Download</span>
            </Button>
          )}

          {/* Fullscreen */}
          {onToggleFullscreen && (
            <Button variant="ghost" size="icon" className="h-8 w-8 rounded-full ml-1 hover:bg-secondary" onClick={onToggleFullscreen}>
              {isFullscreen ? <Minimize2 className="h-4 w-4 text-foreground/80" /> : <Maximize2 className="h-4 w-4 text-foreground/80" />}
            </Button>
          )}
        </div>
      </div>

      {/* Content area */}
      <div className="flex-1 flex overflow-hidden">
        {/* PDF content */}
        <div
            className={`flex-1 overflow-auto bg-secondary/10 ${showSidebar ? '' : 'w-full'}`}
            onScroll={handleScroll}
        >
          {loading && (
            <div className="flex items-center justify-center h-full py-16">
              <Spinner />
              <span className="ml-2 text-muted-foreground">Loading PDF...</span>
            </div>
          )}

          {error && (
            <div className="flex flex-col items-center justify-center h-full py-16 gap-3">
              <p className="text-destructive">{error}</p>
              {onDownload && (
                <Button variant="outline" size="sm" onClick={onDownload}>
                  <Download className="h-4 w-4 mr-1" /> Download instead
                </Button>
              )}
            </div>
          )}

          {!error && (
            <div className="py-8 px-4 sm:px-8 w-fit min-w-full min-h-full flex flex-col items-center">
              <Document
                file={url}
                onLoadSuccess={handleDocumentLoadSuccess}
                onLoadError={handleDocumentLoadError}
                loading={null}
              >
                {Array.from(new Array(numPages), (_, index) => (
                  <div
                    key={`page_${index + 1}`}
                    className="mb-8 shadow-2xl bg-white border border-border/20 overflow-hidden shrink-0 relative rounded-sm"
                    id={`pdf-page-${index + 1}`}
                  >
                    <Page
                      pageNumber={index + 1}
                      scale={scale}
                      renderTextLayer={true}
                      renderAnnotationLayer={true}
                      loading={
                        <div className="w-[800px] h-[1000px] flex items-center justify-center bg-white/50 text-muted-foreground animate-pulse">
                          Loading page {index + 1}...
                        </div>
                      }
                    />
                  </div>
                ))}
              </Document>
            </div>
          )}
        </div>

        {/* Sections sidebar wrapper (Absolute for no squeezing, or relative based on width) */}
        {showSidebar && sections.length > 0 && (
          <div className="absolute right-0 top-0 bottom-0 w-80 max-w-[85%] border-l bg-card/95 backdrop-blur-md overflow-y-auto flex-shrink-0 shadow-2xl z-10 sm:static sm:bg-card sm:shadow-none sm:z-auto transition-all">
            <div className="p-4 border-b bg-card/50 backdrop-blur-md sticky top-0 z-20 flex justify-between items-center">
              <h4 className="text-sm font-bold text-foreground">Course Sections</h4>
              <span className="text-xs font-medium bg-primary/10 text-primary px-2 py-0.5 rounded-full">
                {sections.length}
              </span>
            </div>
            <div className="p-3 space-y-3">
              {sortedSections.map((section) => {
                const isActive = currentSection?.id === section.id;
                const isLoadingThis = loadingSection?.sectionId === section.id;

                return (
                  <SectionCard
                    key={section.id}
                    section={section}
                    isActive={isActive}
                    isLoading={isLoadingThis}
                    loadingType={isLoadingThis ? loadingSection?.type : undefined}
                    onGoToSection={() => goToPage(section.startPage ?? 1)}
                    onSectionAction={onSectionAction}
                  />
                );
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

interface SectionCardProps {
  section: Section;
  isActive: boolean;
  isLoading: boolean;
  loadingType?: string;
  onGoToSection: () => void;
  onSectionAction?: (type: 'quiz' | 'summary' | 'flashcards', sectionId: string) => void;
}

function SectionCard({
  section,
  isActive,
  isLoading,
  loadingType,
  onGoToSection,
  onSectionAction,
}: SectionCardProps) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div
      className={`rounded-xl border transition-all duration-200 overflow-hidden ${
        isActive 
          ? 'bg-primary/5 border-primary/30 shadow-sm' 
          : 'bg-card border-border/50 hover:border-border hover:bg-secondary/30'
      }`}
    >
      <div
        className="p-3.5 cursor-pointer group"
        onClick={onGoToSection}
      >
        <div className="flex items-start justify-between gap-3">
          <div className="flex-1 min-w-0">
            <p className={`text-sm font-semibold leading-snug line-clamp-2 transition-colors ${isActive ? 'text-primary' : 'group-hover:text-foreground/90'}`}>
              {section.title}
            </p>
            <div className="flex items-center gap-2 mt-1.5 opacity-70">
              <BookOpen className="h-3 w-3" />
              <p className="text-[10px] uppercase font-bold tracking-wider">
                {section.startPage && section.endPage
                  ? `Pages ${section.startPage}-${section.endPage}`
                  : section.startPage
                  ? `Page ${section.startPage}`
                  : ''}
              </p>
            </div>
          </div>
          {section.summary && (
            <Button
              variant="ghost"
              size="icon"
              className={`h-7 w-7 flex-shrink-0 rounded-full transition-transform ${expanded ? 'rotate-180 bg-secondary' : 'hover:bg-secondary'}`}
              onClick={(e) => {
                e.stopPropagation();
                setExpanded(!expanded);
              }}
            >
              <ChevronDown className="h-4 w-4" />
            </Button>
          )}
        </div>

        {/* Summary preview */}
        {expanded && section.summary && (
          <div className="mt-3 pt-3 border-t border-border/50">
            <p className="text-xs text-muted-foreground leading-relaxed">
              {section.summary}
            </p>
          </div>
        )}
      </div>

      {/* Action buttons */}
      {onSectionAction && (
        <div className="px-3 pb-3 flex gap-1.5 w-full flex-wrap">
          <Button
            variant={isActive ? "primary" : "outline"}
            size="sm"
            className={`h-8 text-xs flex-1 min-w-[70px] rounded-md ${isActive ? 'shadow-sm shadow-primary/20' : ''}`}
            onClick={(e) => {
              e.stopPropagation();
              onSectionAction('summary', section.id);
            }}
            disabled={isLoading}
            loading={isLoading && loadingType === 'summary'}
          >
            <BookOpen className="h-3.5 w-3.5 mr-1" />
            Summary
          </Button>
          <Button
            variant={isActive ? "primary" : "outline"}
            size="sm"
            className={`h-8 text-xs flex-1 min-w-[70px] rounded-md ${isActive ? 'shadow-sm shadow-primary/20' : ''}`}
            onClick={(e) => {
              e.stopPropagation();
              onSectionAction('quiz', section.id);
            }}
            disabled={isLoading}
            loading={isLoading && loadingType === 'quiz'}
          >
            <FileQuestion className="h-3.5 w-3.5 mr-1" />
            Quiz
          </Button>
          <Button
            variant={isActive ? "primary" : "outline"}
            size="sm"
            className={`h-8 text-xs flex-1 min-w-[70px] rounded-md ${isActive ? 'shadow-sm shadow-primary/20' : ''}`}
            onClick={(e) => {
              e.stopPropagation();
              onSectionAction('flashcards', section.id);
            }}
            disabled={isLoading}
            loading={isLoading && loadingType === 'flashcards'}
          >
            <Lightbulb className="h-3.5 w-3.5 mr-1" />
            Cards
          </Button>
        </div>
      )}
    </div>
  );
}

