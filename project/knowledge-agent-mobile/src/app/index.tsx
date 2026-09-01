import { useMemo, useState } from "react";
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import {
  askKnowledgeAgent,
  type ChatResponse,
} from "@/api/knowledgeAgent";
import { CitationCard } from "@/components/CitationCard";

export default function ChatScreen() {
  const [query, setQuery] = useState("");
  const [resp, setResp] = useState<ChatResponse | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const blockByChunkId = useMemo(
    () => new Map(resp?.blocks.map((b) => [b.chunk_id, b])),
    [resp],
  );

  async function ask() {
    const q = query.trim();
    if (!q || busy) return;
    setBusy(true);
    setError(null);
    try {
      setResp(await askKnowledgeAgent(q));
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e));
    } finally {
      setBusy(false);
    }
  }

  const canSend = query.trim().length > 0 && !busy;

  return (
    <SafeAreaView style={styles.safe} edges={["bottom"]}>
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : undefined}
        style={styles.flex}
      >
        <ScrollView
          contentContainerStyle={styles.content}
          keyboardShouldPersistTaps="handled"
        >
          <View style={styles.inputBox}>
            <TextInput
              value={query}
              onChangeText={setQuery}
              placeholder="輸入問題,例如:什麼是 RAG?"
              placeholderTextColor="#94a3b8"
              multiline
              style={styles.input}
              editable={!busy}
            />
            <Pressable
              onPress={ask}
              disabled={!canSend}
              style={({ pressed }) => [
                styles.button,
                !canSend && styles.buttonDisabled,
                pressed && canSend && styles.buttonPressed,
              ]}
            >
              {busy ? (
                <ActivityIndicator color="#ffffff" />
              ) : (
                <Text style={styles.buttonText}>送出</Text>
              )}
            </Pressable>
          </View>

          {error ? (
            <View style={styles.errorBox}>
              <Text style={styles.errorText}>{error}</Text>
            </View>
          ) : null}

          {resp ? (
            <View style={styles.results}>
              <View style={styles.answerCard}>
                <Text style={styles.answerMeta}>
                  延遲 {resp.latency_ms ?? "?"}ms
                  {resp.expanded_queries.length > 1
                    ? ` · 擴增查詢 ${resp.expanded_queries.length}`
                    : ""}
                </Text>
                <Text style={styles.answerText}>{resp.answer}</Text>
              </View>

              {resp.citations.length > 0 ? (
                <View style={styles.citations}>
                  <Text style={styles.sectionTitle}>引用來源</Text>
                  {resp.citations.map((c) => (
                    <CitationCard
                      key={c.n}
                      citation={c}
                      block={blockByChunkId.get(c.chunk_id)}
                    />
                  ))}
                </View>
              ) : null}
            </View>
          ) : null}

          {!resp && !busy && !error ? (
            <View style={styles.empty}>
              <Text style={styles.emptyText}>
                問一個問題,系統會從已上傳的文件中檢索答案,
                並附上引用與原文 chunk。
              </Text>
            </View>
          ) : null}
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: "#f8fafc" },
  flex: { flex: 1 },
  content: { padding: 16, gap: 16 },

  inputBox: { gap: 10 },
  input: {
    minHeight: 88,
    padding: 12,
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 10,
    backgroundColor: "#ffffff",
    fontSize: 15,
    color: "#0f172a",
    textAlignVertical: "top",
  },
  button: {
    backgroundColor: "#0f172a",
    borderRadius: 10,
    paddingVertical: 12,
    alignItems: "center",
    justifyContent: "center",
  },
  buttonDisabled: { backgroundColor: "#94a3b8" },
  buttonPressed: { opacity: 0.85 },
  buttonText: { color: "#ffffff", fontWeight: "600", fontSize: 15 },

  errorBox: {
    backgroundColor: "#fef2f2",
    borderColor: "#fecaca",
    borderWidth: 1,
    borderRadius: 10,
    padding: 12,
  },
  errorText: { color: "#b91c1c", fontSize: 13 },

  results: { gap: 16 },
  answerCard: {
    backgroundColor: "#ffffff",
    borderWidth: 1,
    borderColor: "#e2e8f0",
    borderRadius: 12,
    padding: 14,
    gap: 8,
  },
  answerMeta: { fontSize: 11, color: "#64748b" },
  answerText: { fontSize: 15, lineHeight: 22, color: "#0f172a" },

  citations: { gap: 8 },
  sectionTitle: { fontSize: 13, fontWeight: "600", color: "#334155" },

  empty: {
    marginTop: 24,
    padding: 16,
    borderRadius: 12,
    borderWidth: 1,
    borderStyle: "dashed",
    borderColor: "#cbd5e1",
    backgroundColor: "#ffffff",
  },
  emptyText: { color: "#64748b", fontSize: 13, lineHeight: 20 },
});
