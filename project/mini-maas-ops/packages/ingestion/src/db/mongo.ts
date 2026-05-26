/**
 * Mongoose 連線管理。
 */
import mongoose from 'mongoose';
import { logger } from '@maas/shared';

export async function connectMongo(url: string): Promise<void> {
  await mongoose.connect(url);
  logger.info('✅ Mongoose connected to MongoDB');
}

export async function disconnectMongo(): Promise<void> {
  await mongoose.disconnect();
  logger.info('🛑 Mongoose disconnected');
}
