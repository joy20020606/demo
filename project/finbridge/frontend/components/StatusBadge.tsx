const tones: Record<string, string> = {
  success: "bg-emerald-100 text-emerald-700",
  replayed: "bg-emerald-100 text-emerald-700",
  resolved: "bg-emerald-100 text-emerald-700",
  acknowledged: "bg-emerald-100 text-emerald-700",
  running: "bg-sky-100 text-sky-700",
  partial: "bg-amber-100 text-amber-700",
  pending: "bg-amber-100 text-amber-700",
  open: "bg-amber-100 text-amber-700",
  failed: "bg-rose-100 text-rose-700",
};

export default function StatusBadge({ status }: { status: string }) {
  const tone = tones[status] ?? "bg-slate-100 text-slate-600";
  return (
    <span
      className={`inline-block rounded px-2 py-0.5 text-xs font-medium ${tone}`}
    >
      {status}
    </span>
  );
}
