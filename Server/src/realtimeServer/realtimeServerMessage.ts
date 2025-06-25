import { create, toBinary } from '@bufbuild/protobuf'
import { PackagedEnvelope, PackagedEnvelopeSchema } from '../protoGen/packagedEnvelope_pb'
import { BroadcastMessageEnvelope } from '../protoGen/broadcastMessage_pb'

export class RealtimeServerMessage {
  private constructor(
    private packagedEnvelope: PackagedEnvelope
  ) {}

  toBinary() {
    return toBinary(PackagedEnvelopeSchema, this.packagedEnvelope)
  }

  private static makeMessageCreator<T extends { content: any }>() {
    return <K extends keyof CaseToValueMapFromUnion<T['content']>>(
      messageType: K,
      payload: DropGrpcFields<CaseToValueMapFromUnion<T['content']>[K]>
    ): RealtimeServerMessage => {
      const packagedEnvelope = create(PackagedEnvelopeSchema, {
        content: {
          case: 'functions',
          value: {
            content: {
              case: messageType,
              value: payload
            } as any
          }
        }
      })

      return new RealtimeServerMessage(packagedEnvelope)
    }
  }

  static createFunctionMessage = RealtimeServerMessage.makeMessageCreator<BroadcastMessageEnvelope>()
}

type CaseToValueMapFromUnion<T> = {
  [U in T extends { case: infer C extends string; value: infer V }
    ? C
    : never]: Extract<T, { case: U }> extends { value: infer V } ? V : never
}