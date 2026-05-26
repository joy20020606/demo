import Anthropic from '@anthropic-ai/sdk';

/**
 * 共用的 Anthropic client
 * 從環境變數讀 API key，避免 hard-code
 */
export function createAnthropicClient(): Anthropic {
  const apiKey = process.env.ANTHROPIC_API_KEY;
  if (!apiKey) {
    throw new Error(
      'ANTHROPIC_API_KEY is not set. ' +
        'Get one from https://console.anthropic.com/settings/keys'
    );
  }
  return new Anthropic({ apiKey });
}

export const DEFAULT_MODEL = process.env.ANTHROPIC_MODEL ?? 'claude-sonnet-4-6';
