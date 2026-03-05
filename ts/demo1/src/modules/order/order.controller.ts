import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { DemoService } from '../demo/demo.service';
import { ResponseContract } from '../../common/contracts';
import { DemoDto } from '../demo/dto';

/**
 * Order Controller — 展示跨模組注入 DemoService
 *
 * 重點：這個 Controller 屬於 OrderModule，
 * 但它注入的是 DemoModule 的 DemoService。
 * 這就是為什麼 DemoModule 需要 exports: [DemoService]
 */
@ApiTags('order')
@Controller('order')
export class OrderController {
  // 注入「別的模組」的 DemoService
  constructor(private readonly demoService: DemoService) {}

  /**
   * 展示跨模組呼叫 — 從 OrderController 呼叫 DemoService
   */
  @Get('demo-data')
  @ApiOperation({ summary: '從 OrderModule 取得 Demo 資料（跨模組注入展示）' })
  @ApiResponse({ status: 200, description: '成功從 DemoService 取得資料' })
  getDemoData(): ResponseContract<DemoDto[]> {
    // 直接使用 DemoModule 的 DemoService
    const data = this.demoService.findAll();
    return ResponseContract.success(data);
  }
}
