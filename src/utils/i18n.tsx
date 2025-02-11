import React, { createContext, PropsWithChildren, useContext, useState } from 'react'
import languages from '~/language'

const i18nContext = createContext<SupportedLanguage>('en-us')

export type SupportedLanguage = 'en-us'

export interface I18nProviderProps {
  defaultLanguage?: SupportedLanguage
}

let setLanguageFn: React.Dispatch<React.SetStateAction<SupportedLanguage>> = null!
let currentLanguage: SupportedLanguage = 'en-us'

export function I18nProvider(props: PropsWithChildren<I18nProviderProps>) {
  const [language, setLanguage] = useState<SupportedLanguage>(props.defaultLanguage ?? currentLanguage)
  setLanguageFn = setLanguage

  return (
    <i18nContext.Provider value={language}>{props.children}</i18nContext.Provider>
  )
}

export function setCurrentLanguage(language: SupportedLanguage) {
  currentLanguage = language
  if (setLanguageFn !== null) setLanguageFn(language)
}

export function useCurrentLanguage() {
  return useContext(i18nContext)
}

export function useI18n() {
  const currentLanguage = useCurrentLanguage()
  return languages[currentLanguage]
}

export function globalI18n() {
  return languages[currentLanguage]
}
