// Encoding and decoding string array composed several (string length(4byte) + string content)
export class StringArrayPacker {
  static pack(...items: (string | null | undefined)[]): Uint8Array {
    if (!items) throw new Error('items is null or undefined')

    const buffers: Buffer[] = []

    for (const item of items) {
      const str = item ?? ''
      const strBuffer = Buffer.from(str, 'utf8')
      const lengthBuffer = Buffer.alloc(4)
      lengthBuffer.writeUInt32LE(strBuffer.length, 0)
      buffers.push(lengthBuffer, strBuffer)
    }

    const divider = Buffer.alloc(4)
    divider.writeUInt32LE(0)
    buffers.push(divider)

    return Buffer.concat(buffers)
  }

  static unpack(data: Uint8Array): string[][] {
    if (!data || data.length === 0) throw new Error('Data is null or empty.')

    const buffer = Buffer.isBuffer(data) ? data : Buffer.from(data)
    const items: string[][] = [[]]
    let offset = 0

    while (offset < buffer.length) {
      if (offset + 4 > buffer.length) {
        throw new Error('Invalid data format: incomplete length prefix.')
      }

      const length = buffer.readUInt32LE(offset)
      offset += 4

      if (length === 0) {
        if (offset < buffer.length) items.push([])
        continue
      }

      if (offset + length > buffer.length) {
        throw new Error('Invalid data format: string bytes exceed data length.')
      }

      const str = buffer.toString('utf8', offset, offset + length)
      items[items.length - 1].push(str)
      offset += length
    }

    return items
  }
}
