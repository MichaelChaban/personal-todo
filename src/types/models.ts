// Enums
export enum MeetingItemStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  UnderReview = 'UnderReview',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Deferred = 'Deferred'
}

export enum MeetingItemOutcome {
  Decision = 'Decision',
  Discussion = 'Discussion',
  Information = 'Information'
}

export enum FieldType {
  Text = 'Text',
  Number = 'Number',
  Date = 'Date',
  YesNo = 'YesNo',
  Dropdown = 'Dropdown',
  MultipleChoice = 'MultipleChoice'
}

// Interfaces
export interface FieldOption {
  value: string
  label: string
  isDefault: boolean
  isActive: boolean
  displayOrder?: number
}

export interface FieldDefinition {
  id: string
  fieldName: string
  label: string
  fieldType: FieldType
  displayOrder: number
  isRequired: boolean
  isActive: boolean
  isTextArea: boolean
  placeholderText: string | null
  helpText: string | null
  options: FieldOption[] | null
}

export interface FieldValues {
  [key: string]: string | number | boolean | string[] | null
}

export interface MeetingItemFieldValue {
  fieldName: string
  textValue: string | null
  numberValue: number | null
  dateValue: string | null
  booleanValue: boolean | null
  jsonValue: string | null
}

export interface Document {
  id: string
  fileName: string
  fileSize: number
  uploadDate: string
  uploadedBy: string
}

export interface MeetingItem {
  id: string | null
  topic: string
  purpose: string
  outcome: MeetingItemOutcome
  duration: number
  digitalProduct: string
  requestor: string
  ownerPresenter: string
  sponsor: string | null
  submissionDate: string
  status: MeetingItemStatus
  decisionBoardId: string
  templateId: string | null
  fieldValues: FieldValues
  documents: Document[]
}

export interface Template {
  id: string | null
  name: string
  description: string | null
  decisionBoardId: string | null
  isActive: boolean
  fieldDefinitions: FieldDefinition[]
  createdAt?: string
  updatedAt?: string
}

export interface DecisionBoard {
  id: string
  name: string
  abbreviation: string
  description: string | null
  isActive: boolean
}

export interface FieldTypeDefinition {
  type: FieldType
  label: string
  icon: string
  color: string
}

export interface HistoricalField {
  fieldName: string
  label: string
  value: string | number | boolean | string[]
  fieldType: FieldType
  wasRequired: boolean
}
