import {
  Controller,
  Get,
  Post,
  Put,
  Delete,
  Body,
  Param,
  ParseIntPipe,
} from '@nestjs/common';
import { ApiTags, ApiOperation, ApiParam, ApiResponse, ApiExtraModels } from '@nestjs/swagger';
import { DemoService } from './demo.service';
import { CreateDemoDto, UpdateDemoDto, DemoDto } from './dto';
import { ResponseContract } from '../../common/contracts';

/**
 * Demo Controller — 類似 C# 的 DemoController
 *
 * 裝飾器對照：
 *   C#: [ApiController], [Route("api/[controller]")]
 *   NestJS: @Controller('demo'), @ApiTags('demo')
 */
@ApiTags('demo')
@ApiExtraModels(DemoDto)
@Controller('demo')
export class DemoController {
  // 依賴注入 — 跟 C# 的 constructor injection 一樣
  constructor(private readonly demoService: DemoService) {}

  /**
   * 取得所有 Demo
   * C# 對照: [HttpGet]
   */
  @Get()
  @ApiOperation({ summary: '取得所有 Demo' })
  @ApiResponse({ status: 200, description: '成功取得所有 Demo' })
  findAll1(): ResponseContract<DemoDto[]> {
    const data = this.demoService.findAll();
    return ResponseContract.success(data);
  }

  /**
   * 依 ID 取得 Demo
   * C# 對照: [HttpGet("{id}")]
   */
  @Get(':id')
  @ApiOperation({ summary: '依 ID 取得 Demo' })
  @ApiParam({ name: 'id', type: Number, description: 'Demo ID' })
  @ApiResponse({ status: 200, description: '成功取得 Demo' })
  @ApiResponse({ status: 404, description: 'Demo 不存在' })
  findOne(@Param('id', ParseIntPipe) id: number): ResponseContract<DemoDto> {
    const data = this.demoService.findOne(id);
    return ResponseContract.success(data);
  }

  /**
   * 建立 Demo
   * C# 對照: [HttpPost]
   */
  @Post()
  @ApiOperation({ summary: '建立 Demo' })
  @ApiResponse({ status: 201, description: '成功建立 Demo' })
  @ApiResponse({ status: 400, description: '請求格式錯誤' })
  create(@Body() dto: CreateDemoDto): ResponseContract<DemoDto> {
    const data = this.demoService.create(dto);
    return ResponseContract.created(data);
  }

  /**
   * 更新 Demo
   * C# 對照: [HttpPut("{id}")]
   */
  @Put(':id')
  @ApiOperation({ summary: '更新 Demo' })
  @ApiParam({ name: 'id', type: Number, description: 'Demo ID' })
  @ApiResponse({ status: 200, description: '成功更新 Demo' })
  @ApiResponse({ status: 404, description: 'Demo 不存在' })
  update(
    @Param('id', ParseIntPipe) id: number,
    @Body() dto: UpdateDemoDto,
  ): ResponseContract<DemoDto> {
    const data = this.demoService.update(id, dto);
    return ResponseContract.success(data, 'Updated');
  }

  /**
   * 刪除 Demo
   * C# 對照: [HttpDelete("{id}")]
   */
  @Delete(':id')
  @ApiOperation({ summary: '刪除 Demo' })
  @ApiParam({ name: 'id', type: Number, description: 'Demo ID' })
  @ApiResponse({ status: 200, description: '成功刪除 Demo' })
  @ApiResponse({ status: 404, description: 'Demo 不存在' })
  remove(@Param('id', ParseIntPipe) id: number): ResponseContract<null> {
    this.demoService.remove(id);
    return ResponseContract.success(null, 'Deleted');
  }
}
