import { ApiProperty } from '@nestjs/swagger';

/**
 * 統一回應格式 — 類似 C# 的 ResponseContract<T>
 */
export class ResponseContract<T> {
  @ApiProperty({ example: 200 })
  statusCode!: number;

  @ApiProperty({ example: 'Success' })
  message!: string;

  @ApiProperty()
  data!: T;

  constructor(statusCode: number, message: string, data: T) {
    this.statusCode = statusCode;
    this.message = message;
    this.data = data;
  }

  /** 成功回應 */
  static success<T>(data: T, message = 'Success'): ResponseContract<T> {
    return new ResponseContract(200, message, data);
  }

  /** 建立成功回應 */
  static created<T>(data: T, message = 'Created'): ResponseContract<T> {
    return new ResponseContract(201, message, data);
  }
}
