import { CreateCustomerForm } from '../../components/CreateCustomerForm';
import { CustomerTable } from '../../components/CustomerTable';

export const metadata = {
  title: '客戶管理 · Mini SME CRM',
};

export default function CustomersPage() {
  return (
    <div className="space-y-6">
      <section>
        <h1 className="text-2xl font-bold text-gray-900">客戶管理</h1>
        <p className="mt-1 text-sm text-gray-600">
          管理你的客戶名單，支援搜尋
        </p>
      </section>

      <CreateCustomerForm />
      <CustomerTable />
    </div>
  );
}
