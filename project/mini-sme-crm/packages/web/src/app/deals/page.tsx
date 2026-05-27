import { CreateDealForm } from '../../components/CreateDealForm';
import { KanbanBoard } from '../../components/KanbanBoard';

export const metadata = {
  title: '商機 Kanban · Mini SME CRM',
};

export default function DealsPage() {
  return (
    <div className="space-y-6">
      <section>
        <h1 className="text-2xl font-bold text-gray-900">商機 Kanban</h1>
        <p className="mt-1 text-sm text-gray-600">
          拖拉卡片切換階段。
          <span className="ml-2 text-amber-700">
            ⚠️ WON / LOST 是終態，無法再轉。
          </span>
        </p>
      </section>

      <CreateDealForm />
      <KanbanBoard />
    </div>
  );
}
