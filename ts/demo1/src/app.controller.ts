import { Controller, Get } from '@nestjs/common';
import { ApiOperation, ApiTags } from '@nestjs/swagger';
import { AppService } from './app.service';

/**
 * 根 Controller — 健康檢查用
 */
@ApiTags('health')
@Controller()
export class AppController {
  constructor(private readonly appService: AppService) {}

  /** 健康檢查端點 */
  @Get('health')
  @ApiOperation({ summary: '健康檢查' })
  healthCheck() {
    return this.appService.healthCheck();
  }
}
