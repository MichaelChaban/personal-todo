import type { DocumentDto } from '../models/meetingItem'

/**
 * Convert a File object to base64 string
 */
export async function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.readAsDataURL(file)
    reader.onload = () => resolve(reader.result as string)
    reader.onerror = (error) => reject(error)
  })
}

/**
 * Convert a File object to DocumentDto
 */
export async function fileToDocumentDto(file: File): Promise<DocumentDto> {
  const base64Content = await fileToBase64(file)

  return {
    fileName: file.name,
    contentType: file.type,
    fileSize: file.size,
    base64Content
  }
}

/**
 * Convert multiple Files to DocumentDto array
 */
export async function filesToDocumentDtos(files: File[]): Promise<DocumentDto[]> {
  return Promise.all(files.map(file => fileToDocumentDto(file)))
}

/**
 * Format file size to human-readable string
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes'

  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))

  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
}

/**
 * Validate file size (max 10 MB)
 */
export function isFileSizeValid(file: File): boolean {
  const maxSize = 10 * 1024 * 1024 // 10 MB
  return file.size > 0 && file.size <= maxSize
}

/**
 * Get file extension from filename
 */
export function getFileExtension(filename: string): string {
  return filename.slice((filename.lastIndexOf('.') - 1 >>> 0) + 2)
}
