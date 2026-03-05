import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { DemoService } from '../demo/demo.service';
import { ResponseContract } from '../../common/contracts';
import { DemoDto } from '../demo/dto';

/**
 * Test1 Controller — 測試用 API
 */
@ApiTags('test1')
@Controller('test1')
export class Test1Controller {
  // 依賴注入 — 跟 C# 的 constructor injection 一樣
  constructor(private readonly demoService: DemoService) {}

  /**
   * 取得所有 Demo
   * C# 對照: [HttpGet]
   */
  @Get('test1-data')
  @ApiOperation({ summary: '取得所有 Demo' })
  @ApiResponse({ status: 200, description: '成功取得所有 Demo' })
  findAll1(): ResponseContract<DemoDto[]> {
    const data = this.demoService.findAll();
    return ResponseContract.success(data);
  }
}