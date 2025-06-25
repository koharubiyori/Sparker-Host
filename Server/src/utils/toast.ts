import { grpcClients } from './grpcClient'

export async function toast(title: string, content: string) {
  await grpcClients.userUtil.showToast({ title, content })
}