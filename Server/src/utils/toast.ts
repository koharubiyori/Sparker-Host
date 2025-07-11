import { grpcClients } from './grpcClient'

export async function toast(title: string, content: string) {
  await grpcClients.desktop.showToast({ title, content })
}