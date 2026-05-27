import type { Metadata } from 'next';
import './globals.css';
import { Providers } from './providers';
import { Header } from '../components/Header';

export const metadata: Metadata = {
  title: 'Mini SME CRM',
  description: 'AI-powered CRM for SMEs',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="zh-TW" suppressHydrationWarning>
      <body className="bg-gray-50 text-gray-900" suppressHydrationWarning>
        <Providers>
          <Header />
          <main className="mx-auto max-w-7xl px-4 py-6">{children}</main>
        </Providers>
      </body>
    </html>
  );
}
