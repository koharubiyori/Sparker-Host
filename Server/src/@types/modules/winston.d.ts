import 'winston'

declare module 'winston' {
  interface PredefinedMeta {
    [key: string]: any
    label?: string | string[]
    error?: Error
  }

  interface LeveledLogMethod {
    (message: string, meta: PredefinedMeta): Logger
  }
}