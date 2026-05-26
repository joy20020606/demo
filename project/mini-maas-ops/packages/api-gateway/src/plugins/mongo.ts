/**
 * MongoDB plugin — api-gateway 連 MongoDB，用來讀取 ingestion 寫入的通勤事件。
 *
 * 注意：api-gateway 只「讀」事件，「寫」是 ingestion 微服務的職責。
 */
import fp from 'fastify-plugin';
import mongoose from 'mongoose';
import { config } from '../config.js';

declare module 'fastify' {
  interface FastifyInstance {
    mongoose: typeof mongoose;
  }
}

export default fp(async (fastify) => {
  await mongoose.connect(config.MONGO_URL);
  fastify.log.info('✅ Mongoose connected to MongoDB');

  fastify.decorate('mongoose', mongoose);

  fastify.addHook('onClose', async () => {
    await mongoose.disconnect();
    fastify.log.info('🛑 Mongoose disconnected');
  });
}, {
  name: 'mongo-plugin',
});
