import { useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";

import type { Citation, RetrievedBlock } from "@/api/knowledgeAgent";

type Props = { citation: Citation; block?: RetrievedBlock };

export function CitationCard({ citation, block }: Props) {
  const [open, setOpen] = useState(false);
  const metaLabel = [
    citation.author ?? undefined,
    citation.page_no != null ? `p.${citation.page_no}` : undefined,
  ]
    .filter(Boolean)
    .join(" · ");

  const hasScores =
    typeof block?.vector_score === "number" ||
    typeof block?.rrf_score === "number";

  return (
    <Pressable
      onPress={() => setOpen((v) => !v)}
      style={({ pressed }) => [styles.card, pressed && styles.pressed]}
    >
      <View style={styles.row}>
        <View style={styles.badge}>
          <Text style={styles.badgeText}>{citation.n}</Text>
        </View>
        <View style={styles.textCol}>
          <Text style={styles.source} numberOfLines={2}>
            {citation.source}
          </Text>
          {metaLabel ? <Text style={styles.meta}>{metaLabel}</Text> : null}
        </View>
        <Text style={styles.chevron}>{open ? "▾" : "▸"}</Text>
      </View>

      {open && block ? (
        <View style={styles.excerpt}>
          <Text style={styles.excerptText}>{block.content}</Text>
          {hasScores ? (
            <Text style={styles.scoreLine}>
              {typeof block.vector_score === "number"
                ? `vector ${block.vector_score.toFixed(3)}`
                : ""}
              {typeof block.rrf_score === "number"
                ? ` · rrf ${block.rrf_score.toFixed(3)}`
                : ""}
            </Text>
          ) : null}
        </View>
      ) : null}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    borderWidth: 1,
    borderColor: "#e2e8f0",
    borderRadius: 10,
    backgroundColor: "#ffffff",
    padding: 12,
  },
  pressed: { opacity: 0.85 },
  row: { flexDirection: "row", alignItems: "center", gap: 10 },
  badge: {
    width: 26,
    height: 26,
    borderRadius: 13,
    backgroundColor: "#0f172a",
    alignItems: "center",
    justifyContent: "center",
  },
  badgeText: { color: "#ffffff", fontWeight: "700", fontSize: 12 },
  textCol: { flex: 1 },
  source: { fontSize: 14, fontWeight: "600", color: "#0f172a" },
  meta: { fontSize: 12, color: "#64748b", marginTop: 2 },
  chevron: { color: "#64748b", fontSize: 16 },
  excerpt: {
    marginTop: 10,
    paddingTop: 10,
    borderTopWidth: 1,
    borderTopColor: "#e2e8f0",
    gap: 6,
  },
  excerptText: { fontSize: 13, color: "#334155", lineHeight: 20 },
  scoreLine: { fontSize: 11, color: "#94a3b8", fontFamily: "monospace" },
});
