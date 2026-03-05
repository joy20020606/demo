import { Module } from '@nestjs/common';
import { DemoModule } from '../demo/demo.module';
import { Test1Controller } from './test1.controller';

@Module({
  imports: [DemoModule],
  controllers: [Test1Controller],
})
export class Test1Module {}
