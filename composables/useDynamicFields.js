import { ref, computed } from 'vue'
import { meetingItemsApi } from '../api/meetingItems'

/**
 * Composable for handling dynamic form fields
 * This will be used when template functionality is implemented
 */
export function useDynamicFields() {
  const fieldDefinitions = ref([])
  const fieldValues = ref({})
  const loading = ref(false)

  /**
   * Load field definitions for a template
   */
  const loadFieldDefinitions = async (templateId = null) => {
    loading.value = true
    try {
      const data = await meetingItemsApi.getFieldDefinitions(templateId)
      fieldDefinitions.value = data
      initializeFieldValues()
    } catch (error) {
      console.error('Failed to load field definitions:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  /**
   * Initialize field values based on definitions
   */
  const initializeFieldValues = () => {
    fieldDefinitions.value.forEach(field => {
      if (!(field.fieldName in fieldValues.value)) {
        fieldValues.value[field.fieldName] = getDefaultValue(field.fieldType)
      }
    })
  }

  /**
   * Get default value based on field type
   */
  const getDefaultValue = (fieldType) => {
    const defaults = {
      Text: '',
      MultipleChoice: null,
      YesNo: null,
      Selection: null,
      FileUpload: []
    }
    return defaults[fieldType] ?? ''
  }

  /**
   * Group fields by category
   */
  const fieldsByCategory = computed(() => {
    return fieldDefinitions.value.reduce((acc, field) => {
      const category = field.category || 'General'
      if (!acc[category]) {
        acc[category] = []
      }
      acc[category].push(field)
      return acc
    }, {})
  })

  /**
   * Get required fields that are not filled
   */
  const missingRequiredFields = computed(() => {
    return fieldDefinitions.value
      .filter(field => field.isRequired)
      .filter(field => {
        const value = fieldValues.value[field.fieldName]
        return !value || (Array.isArray(value) && value.length === 0)
      })
  })

  /**
   * Check if all required fields are filled
   */
  const isValid = computed(() => {
    return missingRequiredFields.value.length === 0
  })

  /**
   * Validate a specific field
   */
  const validateField = (fieldName) => {
    const field = fieldDefinitions.value.find(f => f.fieldName === fieldName)
    if (!field) return { valid: true }

    const value = fieldValues.value[fieldName]

    // Required validation
    if (field.isRequired && (!value || (Array.isArray(value) && value.length === 0))) {
      return {
        valid: false,
        message: `${field.fieldName} is required`
      }
    }

    // Custom validation based on field configuration
    if (field.configuration?.validation) {
      const validation = field.configuration.validation

      // Min/max length for text
      if (validation.minLength && value.length < validation.minLength) {
        return {
          valid: false,
          message: `Minimum length is ${validation.minLength}`
        }
      }

      if (validation.maxLength && value.length > validation.maxLength) {
        return {
          valid: false,
          message: `Maximum length is ${validation.maxLength}`
        }
      }

      // Pattern validation
      if (validation.pattern && !new RegExp(validation.pattern).test(value)) {
        return {
          valid: false,
          message: validation.patternMessage || 'Invalid format'
        }
      }
    }

    return { valid: true }
  }

  /**
   * Get field component name based on type
   */
  const getFieldComponent = (fieldType) => {
    const components = {
      Text: 'TextField',
      MultipleChoice: 'MultipleChoiceField',
      YesNo: 'YesNoField',
      Selection: 'SelectionField',
      FileUpload: 'FileUploadField'
    }
    return components[fieldType] || 'TextField'
  }

  /**
   * Convert field values to API format
   */
  const serializeFieldValues = () => {
    return Object.entries(fieldValues.value).map(([fieldName, value]) => {
      const field = fieldDefinitions.value.find(f => f.fieldName === fieldName)
      return {
        fieldDefinitionId: field?.id,
        fieldName,
        value: typeof value === 'object' ? JSON.stringify(value) : value
      }
    })
  }

  /**
   * Load field values from API format
   */
  const deserializeFieldValues = (serializedValues) => {
    serializedValues.forEach(item => {
      try {
        fieldValues.value[item.fieldName] = JSON.parse(item.value)
      } catch {
        fieldValues.value[item.fieldName] = item.value
      }
    })
  }

  return {
    // State
    fieldDefinitions,
    fieldValues,
    loading,

    // Computed
    fieldsByCategory,
    missingRequiredFields,
    isValid,

    // Methods
    loadFieldDefinitions,
    validateField,
    getFieldComponent,
    serializeFieldValues,
    deserializeFieldValues
  }
}
