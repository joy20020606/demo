import { NestFactory } from '@nestjs/core';
import { ValidationPipe } from '@nestjs/common';
import { SwaggerModule, DocumentBuilder } from '@nestjs/swagger';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // 設定全域 API 前綴 (類似 C# 的 app.UsePathBase("/api"))
  app.setGlobalPrefix('api');

  // 設定全域驗證管線 (類似 C# 的 FluentValidation / DataAnnotations)
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,          // 自動移除 DTO 中未定義的屬性
      forbidNonWhitelisted: true, // 傳入未定義屬性時拋錯
      transform: true,          // 自動型別轉換
    }),
  );

  // 設定 Swagger (類似 C# 的 Swashbuckle)
  const config = new DocumentBuilder()
    .setTitle('Demo API')
    .setDescription('NestJS TypeScript Demo API')
    .setVersion('1.0')
    .addTag('demo', 'Demo CRUD 操作')
    .build();

  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup('api/docs', app, document);

  // 啟動伺服器
  const port = process.env.PORT ?? 3000;
  await app.listen(port);
  console.log(`🚀 Server is running on: http://localhost:${port}`);
  console.log(`📚 Swagger UI: http://localhost:${port}/api/docs`);
}

bootstrap();
