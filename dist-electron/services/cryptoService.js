"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.CryptoService = void 0;
const crypto = __importStar(require("crypto"));
class CryptoService {
    secretKey;
    constructor(secretKey = 'MyNavicatSecretKey2026!') {
        this.secretKey = secretKey;
    }
    getKeyAndIV() {
        const key = crypto.createHash('sha256').update(this.secretKey).digest();
        const iv = crypto.createHash('md5').update(this.secretKey).digest();
        return { key, iv };
    }
    encrypt(plainText) {
        if (!plainText)
            return '';
        try {
            const { key, iv } = this.getKeyAndIV();
            const cipher = crypto.createCipheriv('aes-256-cbc', key, iv);
            let encrypted = cipher.update(plainText, 'utf8', 'base64');
            encrypted += cipher.final('base64');
            return encrypted;
        }
        catch {
            return plainText;
        }
    }
    decrypt(cipherText) {
        if (!cipherText)
            return '';
        try {
            const { key, iv } = this.getKeyAndIV();
            const decipher = crypto.createDecipheriv('aes-256-cbc', key, iv);
            let decrypted = decipher.update(cipherText, 'base64', 'utf8');
            decrypted += decipher.final('utf8');
            return decrypted;
        }
        catch {
            return cipherText;
        }
    }
}
exports.CryptoService = CryptoService;
