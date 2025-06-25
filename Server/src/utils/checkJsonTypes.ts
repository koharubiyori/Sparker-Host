import { ExceptionError } from '../middlewares/errorHandler'
import { ErrorCode } from './errorCode'

class TypeValue {
  constructor(public $$constructor: BaseConstructor) {}
}

type BaseConstructor = ObjectConstructor | ArrayConstructor | StringConstructor | NumberConstructor | BooleanConstructor

class OptionalTypeValue extends TypeValue {
  public required: TypeValue
  
  constructor(constructor: BaseConstructor) { 
    super(constructor)
    this.required = new TypeValue(constructor)
  }
}

export const JsonType = {
  object: new OptionalTypeValue(Object),
  array: new OptionalTypeValue(Array),
  string: new OptionalTypeValue(String),
  number: new OptionalTypeValue(Number),
  boolean: new OptionalTypeValue(Boolean),
}

export function checkJsonTypes<T extends { [key: string]: any }>(
  jsonObj: T, 
  patterns: { [K in keyof T]: TypeValue } | string[]
): T {
  let criticisms = []
  if (Array.isArray(patterns)) {
    for (let key of patterns) {
      if (key !in jsonObj) criticisms.push(`[Missing]: ${key}`)
    }
  } else {
    for(let key in patterns){
      let ptnType = patterns[key]
      let isRequired = !(ptnType instanceof OptionalTypeValue)
  
      if (key in (jsonObj ?? {})) {
        let argType = jsonObj[key].constructor
        argType !== ptnType.$$constructor && criticisms.push(`[Type Mismatch] Parameter "${key}" expected a ${ptnType.$$constructor.name}, but received a ${argType.name}`)
      } else if(isRequired) {
        criticisms.push(`[Missing]: ${key}`)
      }
    }
  }

  if (criticisms.length > 0) {
    criticisms.unshift('Invalid request parameters:')
    throw new ExceptionError({ code: ErrorCode.PARAMETER_ERROR, message: 'Invalid request parameters', data: criticisms })
  }

  return jsonObj
}