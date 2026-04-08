'use client'

import { useEffect, useState } from 'react'

type User = { id: string; name: string | null; email: string; role: string; createdAt: string; orders: { id: string }[] }

export default function AdminMembers() {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetch('/api/users').then((r) => r.json()).then((data) => {
      setUsers(data)
      setLoading(false)
    })
  }, [])

  if (loading) return <div>載入中...</div>

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">會員管理</h1>
      <div className="bg-white rounded-lg shadow-sm overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="text-left px-4 py-3">姓名</th>
              <th className="text-left px-4 py-3">Email</th>
              <th className="text-left px-4 py-3">角色</th>
              <th className="text-left px-4 py-3">訂單數</th>
              <th className="text-left px-4 py-3">加入時間</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id} className="border-t hover:bg-gray-50">
                <td className="px-4 py-3">{user.name || '—'}</td>
                <td className="px-4 py-3">{user.email}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded text-xs ${user.role === 'ADMIN' ? 'bg-red-100 text-red-700' : 'bg-blue-100 text-blue-700'}`}>
                    {user.role === 'ADMIN' ? '管理員' : '會員'}
                  </span>
                </td>
                <td className="px-4 py-3">{user.orders.length}</td>
                <td className="px-4 py-3 text-gray-500">{new Date(user.createdAt).toLocaleDateString('zh-TW')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
