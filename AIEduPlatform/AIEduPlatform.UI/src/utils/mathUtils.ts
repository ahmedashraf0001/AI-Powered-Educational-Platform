export function preprocessMath(text: string): string {
  if (!text) return text;

  const latexIndicators = [
    /\\[a-zA-Z]+/,
    /\^{/,
    /_{/,
    /\\frac/,
    /\\sqrt/,
    /\\sum/,
    /\\int/,
    /\\prod/,
    /\\lim/,
  ];

  let result = text;

  // 1. Replace \[ ... \] with $$ ... $$
  result = result.replace(/\\\[([\s\S]*?)\\\]/g, '$$$$$1$$$$');

  // 2. Replace \( ... \) with $ ... $
  result = result.replace(/\\\(([\s\S]*?)\\\)/g, '$$$1$$');

  // 3. Match block math in [ ... ] safely
  // Usually block math has newlines or equals or big LaTeX commands.
  // We'll avoid things that look like Source tags or simple references,
  // and we'll ignore `[` followed by `]` if it's part of a Markdown link: `[]()`.
  result = result.replace(/\[([^\]]+)\](?!\s*\()/g, (match, inner) => {
    // Exclude Source refs and simple citations
    if (/^\s*(?:Source:|[0-9]+$|video|audio|pdf|document)/i.test(inner)) return match;
    
    const hasLatex = latexIndicators.some(pattern => pattern.test(inner));
    const hasMathPattern = /[=<>]/.test(inner) && (/[\^_]/.test(inner) || /\\/.test(inner));
    
    // Only upgrade to block math if there's LaTeX in it or it resembles a significant equation.
    if (hasLatex || hasMathPattern) {
      return `$$${inner}$$`;
    }
    return match;
  });

  // 4. Match inline math in ( ... )
  result = result.replace(/\(([^()]+)\)/g, (match, inner) => {
    // Avoid accidentally matching normal conversational parens that just happen to have math signs
    const hasLatex = latexIndicators.some(pattern => pattern.test(inner));
    const hasMathPattern = /[=<>]/.test(inner) && (/[\^_]/.test(inner) || /\\/.test(inner));
    if (hasLatex || hasMathPattern) {
      return `$${inner}$`;
    }
    return match;
  });

  return result;
}
