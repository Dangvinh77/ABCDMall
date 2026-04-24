import { useCallback, useRef, useState } from "react";
import { MessageCircle, Send, X } from "lucide-react";
import { api } from "../../core/api/api";
import { ChatbotAnswer } from "./ChatbotAnswer";

const GREETING =
  "Hello! I am your ABCD Mall Assistant. How can I help you today?";

type ChatRole = "user" | "assistant";

type ChatLine = {
  id: string;
  role: ChatRole;
  content: string;
};

function extractErrorMessage(err: unknown): string {
  if (err instanceof Error) {
    return err.message;
  }
  return "Something went wrong. Please try again.";
}

export function ChatbotWidget() {
  const [open, setOpen] = useState(false);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [messages, setMessages] = useState<ChatLine[]>([
    { id: "greet", role: "assistant", content: GREETING },
  ]);
  const listRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = useCallback(() => {
    requestAnimationFrame(() => {
      const el = listRef.current;
      if (el) {
        el.scrollTop = el.scrollHeight;
      }
    });
  }, []);

  const send = useCallback(async () => {
    const trimmed = input.trim();
    if (!trimmed || loading) {
      return;
    }

    const userLine: ChatLine = {
      id: `u-${Date.now()}`,
      role: "user",
      content: trimmed,
    };
    setMessages((m) => [...m, userLine]);
    setInput("");
    setLoading(true);
    scrollToBottom();

    try {
      const data = await api.post<{ answer: string }, { message: string }>(
        "/Chatbot/ask",
        { message: trimmed },
      );
      setMessages((m) => [
        ...m,
        {
          id: `a-${Date.now()}`,
          role: "assistant",
          content: data.answer ?? "",
        },
      ]);
    } catch (e) {
      setMessages((m) => [
        ...m,
        {
          id: `e-${Date.now()}`,
          role: "assistant",
          content: extractErrorMessage(e),
        },
      ]);
    } finally {
      setLoading(false);
      scrollToBottom();
    }
  }, [input, loading, scrollToBottom]);

  return (
    <div className="pointer-events-none fixed bottom-6 right-6 z-[100] flex flex-col items-end gap-3">
      {open && (
        <div
          className="pointer-events-auto flex max-h-[min(520px,70vh)] w-[min(100vw-2rem,380px)] flex-col overflow-hidden rounded-2xl border border-gray-700/80 bg-gray-900 shadow-2xl shadow-black/40"
          role="dialog"
          aria-label="ABCD Mall Assistant"
        >
          <div className="flex items-center justify-between border-b border-gray-800 bg-gray-950/80 px-4 py-3">
            <div className="flex items-center gap-2">
              <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-red-600 to-orange-500 text-white shadow-md">
                <MessageCircle className="h-5 w-5" aria-hidden />
              </span>
              <div>
                <p className="text-sm font-bold text-white">Mall Assistant</p>
                <p className="text-[11px] font-medium text-gray-400">Powered by Gemini</p>
              </div>
            </div>
            <button
              type="button"
              onClick={() => setOpen(false)}
              className="rounded-lg p-2 text-gray-400 transition hover:bg-gray-800 hover:text-white"
              aria-label="Close chat"
            >
              <X className="h-5 w-5" />
            </button>
          </div>

          <div
            ref={listRef}
            className="flex max-h-[340px] flex-col gap-3 overflow-y-auto px-4 py-4"
          >
            {messages.map((msg) => (
              <div
                key={msg.id}
                className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[90%] rounded-2xl px-3.5 py-2.5 ${
                    msg.role === "user"
                      ? "bg-red-600 text-white"
                      : "border border-gray-800 bg-gray-800/80 text-gray-100"
                  }`}
                >
                  {msg.role === "assistant" ? (
                    <ChatbotAnswer text={msg.content} />
                  ) : (
                    <span className="whitespace-pre-wrap text-sm">{msg.content}</span>
                  )}
                </div>
              </div>
            ))}
            {loading && (
              <div className="text-xs font-medium text-gray-500">Thinking…</div>
            )}
          </div>

          <div className="border-t border-gray-800 bg-gray-950/80 p-3">
            <div className="flex gap-2">
              <input
                type="text"
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !e.shiftKey) {
                    e.preventDefault();
                    void send();
                  }
                }}
                placeholder="Ask about shops, events, offers…"
                className="min-w-0 flex-1 rounded-xl border border-gray-700 bg-gray-900 px-3 py-2.5 text-sm text-white placeholder:text-gray-500 focus:border-red-500 focus:outline-none focus:ring-1 focus:ring-red-500"
                disabled={loading}
              />
              <button
                type="button"
                onClick={() => void send()}
                disabled={loading || !input.trim()}
                className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-red-600 to-orange-500 text-white shadow-md transition hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-40"
                aria-label="Send message"
              >
                <Send className="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      )}

      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="pointer-events-auto flex h-14 w-14 items-center justify-center rounded-full bg-gradient-to-br from-red-600 to-orange-500 text-white shadow-lg shadow-red-900/30 ring-4 ring-gray-900/50 transition hover:scale-105 hover:shadow-xl"
        aria-expanded={open}
        aria-label={open ? "Close assistant" : "Open assistant"}
      >
        {open ? <X className="h-6 w-6" /> : <MessageCircle className="h-6 w-6" />}
      </button>
    </div>
  );
}
