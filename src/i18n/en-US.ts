export default {
  common: {
    save: 'Save',
    saveDraft: 'Save Draft',
    cancel: 'Cancel',
    delete: 'Delete',
    edit: 'Edit',
    duplicate: 'Duplicate',
    publish: 'Publish',
    submit: 'Submit',
    loading: 'Loading...',
    required: 'Required',
    active: 'Active',
    inactive: 'Inactive',
    yes: 'Yes',
    no: 'No',
    add: 'Add',
    remove: 'Remove',
    select: 'Select',
    search: 'Search',
    filter: 'Filter',
    actions: 'Actions',
    confirm: 'Confirm',
    close: 'Close'
  },

  meetingItem: {
    title: 'Meeting Item',
    newItem: 'New Meeting Item',
    editItem: 'Edit Meeting Item',
    tabs: {
      general: 'General',
      details: 'Details',
      additionalDetails: 'Additional Details',
      documents: 'Documents'
    },
    fields: {
      topic: 'Topic',
      topicHint: 'Short title for the discussion',
      topicRequired: 'Topic is required',
      purpose: 'Purpose',
      purposeHint: 'Full explanation of the discussion and requested decision',
      purposeRequired: 'Purpose is required',
      outcome: 'Outcome',
      outcomeHint: 'Expected outcome type',
      outcomeRequired: 'Outcome is required',
      duration: 'Duration (minutes)',
      durationHint: 'Estimated discussion time',
      durationRequired: 'Duration is required',
      durationPositive: 'Duration must be positive',
      digitalProduct: 'Digital Product',
      digitalProductHint: 'Related digital product or service',
      digitalProductRequired: 'Digital Product is required',
      requestor: 'Requestor',
      requestorHint: 'KBC ID (auto-filled)',
      requestorRequired: 'Requestor is required',
      ownerPresenter: 'Owner/Presenter',
      ownerPresenterHint: 'KBC ID (defaults to Requestor)',
      ownerPresenterRequired: 'Owner/Presenter is required',
      sponsor: 'Sponsor',
      sponsorHint: 'KBC ID (optional)',
      submissionDate: 'Submission Date',
      status: 'Status',
      statusHint: 'Current status of the meeting item',
      decisionBoard: 'Decision Board'
    },
    sections: {
      basicInformation: 'Basic Information',
      participants: 'Participants',
      systemInformation: 'System Information',
      dynamicFields: 'Template Fields',
      historicalFields: 'Historical Fields'
    },
    outcomes: {
      decision: 'Decision',
      discussion: 'Discussion',
      information: 'Information'
    },
    statuses: {
      draft: 'Draft',
      submitted: 'Submitted',
      underReview: 'Under Review',
      approved: 'Approved',
      rejected: 'Rejected',
      deferred: 'Deferred'
    },
    messages: {
      saveSuccess: 'Meeting item saved successfully',
      saveFailed: 'Failed to save meeting item',
      submitSuccess: 'Meeting item submitted successfully',
      submitFailed: 'Failed to submit meeting item',
      loadFailed: 'Failed to load meeting item',
      confirmCancel: 'Are you sure you want to cancel? Unsaved changes will be lost.',
      noTemplate: 'No custom template found for this decision board',
      noHistoricalFields: 'No historical fields to display'
    },
    emptyStates: {
      noTemplate: 'No custom template is configured for this decision board',
      noHistoricalFields: 'No previous field versions found'
    }
  },

  template: {
    title: 'Template',
    newTemplate: 'New Template',
    editTemplate: 'Edit Template',
    fields: {
      name: 'Template Name',
      nameHint: 'E.g., IT Governance Template',
      nameRequired: 'Template name is required',
      description: 'Description',
      descriptionHint: 'Brief description of this template',
      decisionBoard: 'Decision Board',
      decisionBoardRequired: 'Decision Board is required',
      activeTemplate: 'Active Template',
      totalFields: 'Total Fields',
      requiredFields: 'Required'
    },
    fieldTypes: {
      text: 'Text',
      number: 'Number',
      date: 'Date',
      yesNo: 'Yes/No',
      dropdown: 'Dropdown',
      multipleChoice: 'Multiple Choice'
    },
    fieldProperties: {
      fieldName: 'Field Name (ID)',
      fieldNameHint: 'Unique identifier (camelCase)',
      fieldNameInvalid: 'Must start with lowercase letter and contain only letters and numbers',
      fieldNameRequired: 'Field name is required',
      label: 'Label',
      labelHint: 'Display label for the field',
      labelRequired: 'Label is required',
      fieldType: 'Field Type',
      fieldTypeRequired: 'Field type is required',
      requiredField: 'Required Field',
      multilineText: 'Multi-line Text',
      placeholderText: 'Placeholder Text',
      placeholderTextHint: 'Example text shown when field is empty',
      helpText: 'Help Text',
      helpTextHint: 'Additional guidance for users',
      options: 'Options',
      optionValue: 'Value',
      optionLabel: 'Label',
      optionDefault: 'Default',
      addOption: 'Add Option',
      removeOption: 'Remove Option',
      noOptions: 'No options defined'
    },
    sections: {
      templateSettings: 'Template Settings',
      fieldTypes: 'Field Types',
      templateFields: 'Template Fields',
      fieldProperties: 'Field Properties',
      statistics: 'Statistics'
    },
    messages: {
      saveDraftSuccess: 'Template draft saved successfully',
      saveDraftFailed: 'Failed to save template draft',
      publishSuccess: 'Template published successfully',
      publishFailed: 'Failed to publish template',
      loadSuccess: 'Template loaded successfully',
      loadFailed: 'Failed to load template',
      fieldDeleted: 'Field deleted',
      confirmDelete: 'Are you sure you want to delete this field?',
      confirmCancel: 'Are you sure you want to cancel? Unsaved changes will be lost.'
    },
    emptyStates: {
      noFields: 'Drag fields from the left panel to get started',
      noFieldSelected: 'Select a field to edit its properties',
      loadingTemplate: 'Loading template...'
    },
    hints: {
      dragToAdd: 'Drag field types to the canvas to add them to your template',
      fieldCount: '{count} field(s)'
    }
  },

  documents: {
    title: 'Documents',
    upload: 'Upload Document',
    uploadHint: 'Drag and drop files here or click to browse',
    fileName: 'File Name',
    fileSize: 'File Size',
    uploadDate: 'Upload Date',
    uploadedBy: 'Uploaded By',
    noDocuments: 'No documents uploaded yet',
    messages: {
      uploadSuccess: 'Document uploaded successfully',
      uploadFailed: 'Failed to upload document',
      deleteSuccess: 'Document deleted successfully',
      deleteFailed: 'Failed to delete document'
    }
  }
}
