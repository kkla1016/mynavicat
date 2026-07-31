import * as crypto from 'crypto';

export class CryptoService {
  private secretKey: string;

  constructor(secretKey: string = 'MyNavicatSecretKey2026!') {
    this.secretKey = secretKey;
  }

  private getKeyAndIV(): { key: Buffer; iv: Buffer } {
    const key = crypto.createHash('sha256').update(this.secretKey).digest();
    const iv = crypto.createHash('md5').update(this.secretKey).digest();
    return { key, iv };
  }

  public encrypt(plainText: string): string {
    if (!plainText) return '';
    try {
      const { key, iv } = this.getKeyAndIV();
      const cipher = crypto.createCipheriv('aes-256-cbc', key, iv);
      let encrypted = cipher.update(plainText, 'utf8', 'base64');
      encrypted += cipher.final('base64');
      return encrypted;
    } catch {
      return plainText;
    }
  }

  public decrypt(cipherText: string): string {
    if (!cipherText) return '';
    try {
      const { key, iv } = this.getKeyAndIV();
      const decipher = crypto.createDecipheriv('aes-256-cbc', key, iv);
      let decrypted = decipher.update(cipherText, 'base64', 'utf8');
      decrypted += decipher.final('utf8');
      return decrypted;
    } catch {
      return cipherText;
    }
  }
}
