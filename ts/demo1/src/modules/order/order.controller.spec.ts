import { Test, TestingModule } from '@nestjs/testing';
import { OrderController } from './order.controller';
import { DemoService } from '../demo/demo.service';
import { ResponseContract } from '../../common/contracts';
import { DemoDto } from '../demo/dto';

const mockDemos: DemoDto[] = [
  {
    id: 1,
    title: 'Hello NestJS',
    description: '第一個 NestJS Demo',
    createTimestamp: 1000000,
    updateTimestamp: 1000000,
  },
  {
    id: 2,
    title: 'TypeScript is Great',
    description: '從 C# 轉到 TypeScript',
    createTimestamp: 1000001,
    updateTimestamp: 1000001,
  },
];

const mockDemoService = {
  findAll: jest.fn(),
};

describe('OrderController', () => {
  let controller: OrderController;

  beforeEach(async () => {
    const module: TestingModule = await Test.createTestingModule({
      controllers: [OrderController],
      providers: [{ provide: DemoService, useValue: mockDemoService }],
    }).compile();

    controller = module.get<OrderController>(OrderController);
    jest.clearAllMocks();
  });

  describe('getDemoData', () => {
    it('test_getDemoData_returns_success_response_with_data', () => {
      mockDemoService.findAll.mockReturnValue(mockDemos);

      const result = controller.getDemoData();

      expect(result).toBeInstanceOf(ResponseContract);
      expect(result.statusCode).toBe(200);
      expect(result.message).toBe('Success');
      expect(result.data).toEqual(mockDemos);
    });

    it('test_getDemoData_calls_demoService_findAll', () => {
      mockDemoService.findAll.mockReturnValue(mockDemos);

      controller.getDemoData();

      expect(mockDemoService.findAll).toHaveBeenCalledTimes(1);
    });

    it('test_getDemoData_returns_empty_array_when_no_data', () => {
      mockDemoService.findAll.mockReturnValue([]);

      const result = controller.getDemoData();

      expect(result.statusCode).toBe(200);
      expect(result.data).toEqual([]);
    });
  });
});
