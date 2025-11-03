# Meeting Items Management Application

Modern, extensible meeting item form built with Vue 3, Vite, and Quasar.

## 🚀 Quick Start

```bash
# Install dependencies (already done)
npm install

# Start development server
npm run dev
```

The application will be available at **http://localhost:5173**

## 📁 Project Structure

```
VueProject/
├── src/
│   ├── pages/                    # Page components
│   │   ├── HomePage.vue          # Dashboard
│   │   ├── MeetingItemsListPage.vue  # List view
│   │   ├── MeetingItemPage.vue   # Create/Edit form (MAIN)
│   │   └── ComingSoonPage.vue
│   ├── components/
│   │   └── DocumentUpload.vue    # Document management
│   ├── api/
│   │   └── meetingItems.js       # API service
│   ├── composables/
│   │   └── useDynamicFields.js   # Future: dynamic forms
│   ├── router/
│   │   └── index.js              # Routes
│   ├── App.vue                   # Main layout
│   └── main.js                   # Entry point
└── ...
```

## ✨ Features

### Meeting Item Form (Tabbed Interface)
1. **General Tab** - Core fields (Topic, Purpose, Outcome, etc.)
2. **Details Tab** - Additional info and future dynamic fields
3. **Documents Tab** - Upload with versioning `DB20250115TOPIC.v01`

### Other Pages
- Dashboard with statistics
- List view with filters and search
- Responsive navigation drawer

## 🔌 API Integration

Update `src/api/meetingItems.js`:
```javascript
const API_BASE_URL = 'https://your-backend.com/api'
```

Then replace TODO comments in components with API calls.

## 🎯 Status Workflow

```
Submitted → Proposed → Planned → Discussed
              ↓
            Denied
```

## 🚧 Future: Dynamic Templates

Architecture ready for template system:
- Field definitions via API
- `useDynamicFields.js` composable
- Easy field component extension

## 📱 Responsive

Works on desktop, tablet, and mobile devices.

## 📚 Tech Stack

- Vue 3 (Composition API)
- Vite (Build tool)
- Quasar (UI framework)
- Vue Router

---

**Ready to run! Start with `npm run dev`**
