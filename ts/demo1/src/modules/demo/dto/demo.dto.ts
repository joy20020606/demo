import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsNotEmpty, IsOptional, IsString } from 'class-validator';

/**
 * Demo 回應 DTO — 類似 C# 的 DemoContract
 */
export class DemoDto {
  @ApiProperty({ example: 1, description: 'Demo ID' })
  id!: number;

  @ApiProperty({ example: 'Hello NestJS', description: '標題' })
  title!: string;

  @ApiProperty({ example: '這是一個 NestJS Demo', description: '描述' })
  description!: string;

  @ApiProperty({ example: 1709539200000, description: '建立時間 (Unix Timestamp 毫秒)' })
  createTimestamp!: number;

  @ApiProperty({ example: 1709539200000, description: '更新時間 (Unix Timestamp 毫秒)' })
  updateTimestamp!: number;
}

/**
 * 建立 Demo 請求 DTO — 類似 C# 的 DemoCreationContract
 */
export class CreateDemoDto {
  @ApiProperty({ example: 'Hello NestJS', description: '標題' })
  @IsNotEmpty({ message: 'title 不可為空' })
  @IsString()
  title!: string;

  @ApiPropertyOptional({ example: '這是一個 NestJS Demo', description: '描述' })
  @IsOptional()
  @IsString()
  description?: string;
}

/**
 * 更新 Demo 請求 DTO — 類似 C# 的 DemoUpdateContract
 */
export class UpdateDemoDto {
  @ApiPropertyOptional({ example: '更新標題', description: '標題' })
  @IsOptional()
  @IsString()
  title?: string;

  @ApiPropertyOptional({ example: '更新描述', description: '描述' })
  @IsOptional()
  @IsString()
  description?: string;
}
