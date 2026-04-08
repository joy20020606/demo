import { prisma } from '@/lib/prisma'

export default async function AdminDashboard() {
  const [productCount, orderCount, userCount, pendingOrders] = await Promise.all([
    prisma.product.count({ where: { isActive: true } }),
    prisma.order.count(),
    prisma.user.count({ where: { role: 'CUSTOMER' } }),
    prisma.order.count({ where: { status: 'PENDING' } }),
  ])

  const recentOrders = await prisma.order.findMany({
    take: 5,
    orderBy: { createdAt: 'desc' },
    include: { user: { select: { name: true, email: true } } },
  })

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Dashboard</h1>
      <div className="grid grid-cols-4 gap-4 mb-8">
        {[
          { label: '上架商品', value: productCount },
          { label: '總訂單', value: orderCount },
          { label: '會員數', value: userCount },
          { label: '待處理訂單', value: pendingOrders },
        ].map((stat) => (
          <div key={stat.label} className="bg-white rounded-lg p-6 shadow-sm">
            <p className="text-sm text-gray-500">{stat.label}</p>
            <p className="text-3xl font-bold mt-1">{stat.value}</p>
          </div>
        ))}
      </div>
      <div className="bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-lg font-semibold mb-4">最新訂單</h2>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b">
              <th className="text-left py-2">訂單ID</th>
              <th className="text-left py-2">會員</th>
              <th className="text-left py-2">金額</th>
              <th className="text-left py-2">狀態</th>
              <th className="text-left py-2">時間</th>
            </tr>
          </thead>
          <tbody>
            {recentOrders.map((order) => (
              <tr key={order.id} className="border-b hover:bg-gray-50">
                <td className="py-2 font-mono text-xs">{order.id.slice(0, 8)}...</td>
                <td className="py-2">{order.user.name || order.user.email}</td>
                <td className="py-2">NT${order.totalPrice.toFixed(0)}</td>
                <td className="py-2">
                  <span className="px-2 py-0.5 rounded text-xs bg-yellow-100 text-yellow-700">
                    {order.status}
                  </span>
                </td>
                <td className="py-2 text-gray-500">
                  {new Date(order.createdAt).toLocaleDateString('zh-TW')}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
