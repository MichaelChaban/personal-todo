import * as yup from 'yup'

export const meetingItemSchema = yup.object({
  topic: yup
    .string()
    .required('Topic is required')
    .min(1, 'Topic must be at least 1 character')
    .max(200, 'Topic must not exceed 200 characters'),

  purpose: yup
    .string()
    .required('Purpose is required')
    .min(10, 'Purpose must be at least 10 characters')
    .max(2000, 'Purpose must not exceed 2000 characters'),

  outcome: yup
    .string()
    .required('Outcome is required')
    .oneOf(['Decision', 'Discussion', 'Information'], 'Invalid outcome value'),

  duration: yup
    .number()
    .required('Duration is required')
    .min(1, 'Duration must be greater than 0')
    .max(480, 'Duration must not exceed 480 minutes')
    .integer('Duration must be a whole number')
    .typeError('Duration must be a number'),

  digitalProduct: yup
    .string()
    .required('Digital Product is required')
    .min(1, 'Digital Product must be at least 1 character')
    .max(200, 'Digital Product must not exceed 200 characters'),

  requestor: yup
    .string()
    .required('Requestor is required')
    .max(50, 'Requestor must not exceed 50 characters'),

  ownerPresenter: yup
    .string()
    .required('Owner/Presenter is required')
    .max(50, 'Owner/Presenter must not exceed 50 characters'),

  sponsor: yup
    .string()
    .nullable()
    .max(50, 'Sponsor must not exceed 50 characters'),

  decisionBoardId: yup.string().required('Decision Board is required'),

  status: yup
    .string()
    .required('Status is required')
    .oneOf(
      ['Draft', 'Submitted', 'UnderReview', 'Approved', 'Rejected', 'Deferred'],
      'Invalid status value'
    )
})

export type MeetingItemFormData = yup.InferType<typeof meetingItemSchema>
