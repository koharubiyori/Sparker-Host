export class CaptchaManager {
  private static life = 5 * 60 * 1000;
  private static sessions: Record<string, Captcha> = {};

  private static generate(): Captcha {
    const code = Math.random().toString().substring(2, 6).toUpperCase()
    return { code }
  }

  public static get(sessionId: string): Captcha {
    const captcha = this.generate()
    this.sessions[sessionId] = captcha
    setTimeout(() => delete this.sessions[sessionId], this.life)
    return captcha
  }

  public static validate(sessionId: string, captcha: Captcha): boolean {
    const sessionCaptcha = this.sessions[sessionId]
    if (!sessionCaptcha) return false
    if (sessionCaptcha.code !== captcha.code) return false
    delete this.sessions[sessionId]
    return true
  }
}