import { Fragment, type ReactNode } from "react";
import { Link } from "react-router-dom";

const linkTokenRegex = /\[link:([^\]]+)\]/g;

function resolveLinkTarget(token: string): { to: string; label: string } | null {
  const inner = token.trim();
  if (!inner) {
    return null;
  }

  if (inner === "events") {
    return { to: "/events", label: "Events" };
  }

  if (inner === "shops") {
    return { to: "/shops", label: "Shops" };
  }

  if (inner.startsWith("shop:")) {
    const slug = inner.slice("shop:".length).trim();
    if (!slug) {
      return null;
    }
    return { to: `/shops/${encodeURIComponent(slug)}`, label: "View detail" };
  }

  if (inner.includes(":")) {
    return null;
  }

  if (inner === "movies") {
    return { to: "/movies", label: "Cinema" };
  }

  if (inner === "food-court" || inner === "food") {
    return { to: "/food-court", label: "Food court" };
  }

  return { to: `/shops/${encodeURIComponent(inner)}`, label: "View detail" };
}

type ChatbotAnswerProps = {
  text: string;
};

export function ChatbotAnswer({ text }: ChatbotAnswerProps) {
  const nodes: ReactNode[] = [];
  let lastIndex = 0;
  let match: RegExpExecArray | null;
  let key = 0;
  const re = new RegExp(linkTokenRegex.source, linkTokenRegex.flags);

  while ((match = re.exec(text)) !== null) {
    const [full, inner] = match;
    const start = match.index;
    if (start > lastIndex) {
      nodes.push(
        <span key={`t-${key++}`} className="whitespace-pre-wrap">
          {text.slice(lastIndex, start)}
        </span>,
      );
    }

    const target = resolveLinkTarget(inner);
    if (target) {
      nodes.push(
        <span key={`l-${key++}`} className="inline-flex items-center gap-2 align-middle">
          <Link
            to={target.to}
            className="inline-flex shrink-0 items-center rounded-lg bg-gradient-to-r from-red-600 to-orange-500 px-2.5 py-1 text-xs font-bold uppercase tracking-wide text-white shadow-sm transition hover:opacity-90"
          >
            {target.label}
          </Link>
        </span>,
      );
    } else {
      nodes.push(
        <span key={`x-${key++}`} className="whitespace-pre-wrap text-gray-500">
          {full}
        </span>,
      );
    }

    lastIndex = start + full.length;
  }

  if (lastIndex < text.length) {
    nodes.push(
      <span key={`t-${key++}`} className="whitespace-pre-wrap">
        {text.slice(lastIndex)}
      </span>,
    );
  }

  if (nodes.length === 0) {
    return <span className="whitespace-pre-wrap text-gray-200">{text}</span>;
  }

  return (
    <span className="text-sm leading-relaxed text-gray-200">
      {nodes.map((n, i) => (
        <Fragment key={i}>{n}</Fragment>
      ))}
    </span>
  );
}
