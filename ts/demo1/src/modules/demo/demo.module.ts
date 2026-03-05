import { Global, Module } from '@nestjs/common';
import { DemoController } from './demo.controller';
import { DemoService } from './demo.service';

/**
 * Demo 模組 — 類似 C# 在 Program.cs 中註冊 Service
 *
 * C# 對照:
 *   builder.Services.AddScoped<IDemoRepoService, DemoRepoService>();
 *
 * NestJS:
 *   providers: [DemoService]  → 註冊 Service (預設 Singleton)
 *   controllers: [DemoController] → 註冊 Controller
 */
@Global()
@Module({
  controllers: [DemoController],
  providers: [DemoService],
  exports: [DemoService],
})
export class DemoModule {}
