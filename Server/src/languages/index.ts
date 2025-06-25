import { en_us } from './en-us' 
import { osLocale } from 'os-locale'

const i18nList = {
  'en-US': en_us
}

export type SupportedLanguage = keyof typeof i18nList
export type I18n = typeof en_us

const localePromise = osLocale()

export async function getI18n(): Promise<I18n> {
  const locale = (await localePromise) as SupportedLanguage
  return i18nList[locale] ?? i18nList['en-US'] 
}