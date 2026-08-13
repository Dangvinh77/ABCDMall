import { useEffect, useRef, useState } from "react";
import { Calendar } from "lucide-react";

interface AdminDateInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
}

function pad(value: number) {
  return value.toString().padStart(2, "0");
}

function isValidDateParts(day: number, month: number, year: number) {
  const date = new Date(year, month - 1, day);
  return (
    date.getFullYear() === year &&
    date.getMonth() === month - 1 &&
    date.getDate() === day
  );
}

export function formatDateForDisplay(value?: string | null) {
  if (!value) {
    return "";
  }

  const slashMatch = value.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
  if (slashMatch) {
    return value;
  }

  const dateOnlyMatch = value.match(/^(\d{4})-(\d{2})-(\d{2})$/);
  if (dateOnlyMatch) {
    return `${dateOnlyMatch[3]}/${dateOnlyMatch[2]}/${dateOnlyMatch[1]}`;
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return `${pad(parsed.getDate())}/${pad(parsed.getMonth() + 1)}/${parsed.getFullYear()}`;
}

export function parseDisplayDateToPickerValue(value: string) {
  const slashMatch = value.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
  if (!slashMatch) {
    return "";
  }

  const day = Number(slashMatch[1]);
  const month = Number(slashMatch[2]);
  const year = Number(slashMatch[3]);

  if (!isValidDateParts(day, month, year)) {
    return "";
  }

  return `${year}-${pad(month)}-${pad(day)}`;
}

export function parseDisplayDateToIsoBoundary(
  value: string,
  boundary: "start" | "end",
) {
  const pickerValue = parseDisplayDateToPickerValue(value);
  if (!pickerValue) {
    return undefined;
  }

  const time = boundary === "start" ? "T00:00:00" : "T23:59:59.999";
  return new Date(`${pickerValue}${time}`).toISOString();
}

export function AdminDateInput({
  value,
  onChange,
  placeholder = "dd/mm/yyyy",
  className = "",
}: AdminDateInputProps) {
  const [textValue, setTextValue] = useState(value);
  const pickerRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    setTextValue(value);
  }, [value]);

  function handleTextChange(nextValue: string) {
    const sanitized = nextValue.replace(/[^\d/]/g, "").slice(0, 10);
    setTextValue(sanitized);
    onChange(sanitized);
  }

  function openPicker() {
    const picker = pickerRef.current;
    if (!picker) {
      return;
    }

    if (typeof picker.showPicker === "function") {
      picker.showPicker();
      return;
    }

    picker.click();
  }

  return (
    <div className={`flex min-w-0 items-center gap-2 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 ${className}`.trim()}>
      <input
        value={textValue}
        onChange={(event) => handleTextChange(event.target.value)}
        placeholder={placeholder}
        inputMode="numeric"
        className="min-w-0 flex-1 bg-transparent text-sm text-white outline-none placeholder:text-gray-500"
      />
      <button
        type="button"
        onClick={openPicker}
        className="rounded-xl border border-white/10 p-2 text-cyan-200 transition hover:border-cyan-300/50 hover:text-cyan-100"
        aria-label="Open calendar"
        title="Open calendar"
      >
        <Calendar className="size-4" />
      </button>
      <input
        ref={pickerRef}
        type="date"
        value={parseDisplayDateToPickerValue(textValue)}
        onChange={(event) => onChange(formatDateForDisplay(event.target.value))}
        className="sr-only"
        tabIndex={-1}
        aria-hidden="true"
      />
    </div>
  );
}
