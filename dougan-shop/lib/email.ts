import nodemailer from 'nodemailer'

const transporter = nodemailer.createTransport({
  service: 'gmail',
  auth: {
    user: process.env.GMAIL_USER,
    pass: process.env.GMAIL_APP_PASSWORD,
  },
})

type OrderItem = {
  quantity: number
  price: number
  product: { name: string }
}

type Order = {
  id: string
  totalPrice: number
  address: string
  phone: string
  items: OrderItem[]
}

export async function sendOrderConfirmationEmail(order: Order, customerEmail: string, customerName: string | null) {
  const itemsHtml = order.items
    .map(
      (item) =>
        `<tr>
          <td style="padding:8px;border-bottom:1px solid #eee">${item.product.name}</td>
          <td style="padding:8px;border-bottom:1px solid #eee;text-align:center">${item.quantity}</td>
          <td style="padding:8px;border-bottom:1px solid #eee;text-align:right">NT$${(item.price * item.quantity).toFixed(0)}</td>
        </tr>`
    )
    .join('')

  await transporter.sendMail({
    from: `"豆干專賣店" <${process.env.GMAIL_USER}>`,
    to: customerEmail,
    subject: `訂單確認 #${order.id.slice(0, 8)}`,
    html: `
      <div style="font-family:sans-serif;max-width:600px;margin:0 auto">
        <h2 style="color:#92400e">感謝您的訂購！</h2>
        <p>親愛的 ${customerName || '顧客'}，您的訂單已成立。</p>
        <table style="width:100%;border-collapse:collapse">
          <thead>
            <tr style="background:#fef3c7">
              <th style="padding:8px;text-align:left">商品</th>
              <th style="padding:8px;text-align:center">數量</th>
              <th style="padding:8px;text-align:right">小計</th>
            </tr>
          </thead>
          <tbody>${itemsHtml}</tbody>
        </table>
        <p style="text-align:right;font-size:18px;font-weight:bold;color:#92400e">
          總計：NT$${order.totalPrice.toFixed(0)}
        </p>
        <hr/>
        <p><strong>送貨地址：</strong>${order.address}</p>
        <p><strong>聯絡電話：</strong>${order.phone}</p>
        <p style="color:#6b7280;font-size:12px">豆干專賣店 感謝您的支持！</p>
      </div>
    `,
  })
}

export async function sendNewOrderNotificationEmail(order: Order, customerEmail: string) {
  await transporter.sendMail({
    from: `"豆干專賣店系統" <${process.env.GMAIL_USER}>`,
    to: process.env.GMAIL_USER,
    subject: `新訂單通知 #${order.id.slice(0, 8)}`,
    html: `
      <div style="font-family:sans-serif">
        <h2>新訂單進來了！</h2>
        <p><strong>客戶：</strong>${customerEmail}</p>
        <p><strong>金額：</strong>NT$${order.totalPrice.toFixed(0)}</p>
        <p><strong>地址：</strong>${order.address}</p>
        <p><strong>電話：</strong>${order.phone}</p>
        <p><strong>訂單ID：</strong>${order.id}</p>
      </div>
    `,
  })
}
