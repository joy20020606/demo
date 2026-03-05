import { Module } from '@nestjs/common';

import { AppService } from './app.service';
import { DemoModule } from './modules/demo/demo.module';
import { OrderModule } from './modules/order/order.module';
import { Test1Module } from './modules/test1/test1.module';

/**
 * 根模組 — 類似 C# 的 Program.cs 中 DI 容器設定
 * 所有子模組都在這裡註冊
 */
@Module({
  imports: [DemoModule, OrderModule, Test1Module], 
  providers: [AppService],
})
export class AppModule {}
