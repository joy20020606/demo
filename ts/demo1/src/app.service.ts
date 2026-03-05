import { Injectable } from '@nestjs/common';

/**
 * 根 Service
 */
@Injectable()
export class AppService {
  /** 健康檢查 */
  healthCheck() {
    return {
      statusCode: 200,
      message: 'Demo API is running',
      data: {
        timestamp: new Date().toISOString(),
      },
    };
  }
}
