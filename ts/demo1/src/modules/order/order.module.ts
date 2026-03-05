import { Module } from '@nestjs/common';
import { DemoModule } from '../demo/demo.module';
import { OrderController } from './order.controller';

/**
 * Order 模組 — 展示跨模組依賴
 *
 * 重點：imports: [DemoModule]
 * 因為 OrderController 需要注入 DemoService，
 * 而 DemoService 屬於 DemoModule，
 * 所以必須 import DemoModule。
 *
 * 同時，DemoModule 也必須 exports: [DemoService]，
 * 否則 NestJS 會報錯！
 */
@Module({
//   imports: [DemoModule],        // ← 引入 DemoModule，才能用它的 DemoService
  controllers: [OrderController],
})
export class OrderModule {}
