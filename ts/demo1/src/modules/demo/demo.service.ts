import { Injectable, NotFoundException } from '@nestjs/common';
import { DemoDto, CreateDemoDto, UpdateDemoDto } from './dto';

/**
 * Demo Service — 類似 C# 的 DemoRepoService
 * 目前使用 In-Memory 存儲，之後可替換為 TypeORM / Prisma
 */
@Injectable()
export class DemoService {
  /** In-Memory 資料存儲 (之後會換成資料庫) */
  private demos: DemoDto[] = [
    {
      id: 1,
      title: 'Hello NestJS',
      description: '第一個 NestJS Demo',
      createTimestamp: Date.now(),
      updateTimestamp: Date.now(),
    },
    {
      id: 2,
      title: 'TypeScript is Great',
      description: '從 C# 轉到 TypeScript',
      createTimestamp: Date.now(),
      updateTimestamp: Date.now(),
    },
  ];

  private nextId = 3;

  /** 取得所有 Demo */
  findAll(): DemoDto[] {
    return this.demos;
  }

  /** 依 ID 取得 Demo */
  findOne(id: number): DemoDto {
    const demo = this.demos.find((d) => d.id === id);
    if (!demo) {
      throw new NotFoundException(`Demo with ID ${id} not found`);
    }
    return demo;
  }

  /** 建立 Demo */
  create(dto: CreateDemoDto): DemoDto {
    const now = Date.now();
    const newDemo: DemoDto = {
      id: this.nextId++,
      title: dto.title,
      description: dto.description ?? '',
      createTimestamp: now,
      updateTimestamp: now,
    };
    this.demos.push(newDemo);
    return newDemo;
  }

  /** 更新 Demo */
  update(id: number, dto: UpdateDemoDto): DemoDto {
    const demo = this.findOne(id);
    if (dto.title !== undefined) demo.title = dto.title;
    if (dto.description !== undefined) demo.description = dto.description;
    demo.updateTimestamp = Date.now();
    return demo;
  }

  /** 刪除 Demo */
  remove(id: number): void {
    const index = this.demos.findIndex((d) => d.id === id);
    if (index === -1) {
      throw new NotFoundException(`Demo with ID ${id} not found`);
    }
    this.demos.splice(index, 1);
  }
}
