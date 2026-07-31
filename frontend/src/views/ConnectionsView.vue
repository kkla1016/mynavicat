<template>
  <div class="page-container">
    <div class="toolbar">
      <el-button type="primary" :icon="Plus" @click="handleOpenAddModal">
        新增連線
      </el-button>
      <el-button :icon="Refresh" @click="connectionStore.fetchConnections">
        重新整理
      </el-button>
    </div>

    <!-- 連線列表表格 -->
    <el-card class="table-card">
      <el-table
        :data="connectionStore.connections"
        v-loading="connectionStore.loading"
        style="width: 100%"
      >
        <el-table-column prop="name" label="連線名稱" min-width="150" />
        <el-table-column prop="dbType" label="類型" width="120">
          <template #default="{ row }">
            <el-tag :type="getDbTypeTagType(row.dbType)">{{ row.dbType }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="host" label="主機 IP / 位置" min-width="160" />
        <el-table-column prop="port" label="Port" width="90" />
        <el-table-column prop="databaseName" label="預設資料庫" width="130" />
        <el-table-column prop="username" label="使用者" width="120" />
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <el-button
              size="small"
              type="success"
              plain
              :loading="testingId === row.id"
              @click="handleTestConnection(row.id)"
            >
              測試
            </el-button>
            <el-button size="small" type="primary" plain @click="handleOpenEditModal(row)">
              編輯
            </el-button>
            <el-popconfirm title="確定要刪除此連線設定嗎？" @confirm="handleDelete(row.id)">
              <template #reference>
                <el-button size="small" type="danger" plain>刪除</el-button>
              </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增 / 編輯連線對話框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '編輯資料庫連線' : '新增資料庫連線'"
      width="580px"
      destroy-on-close
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="110px"
        style="padding-right: 20px"
      >
        <el-form-item label="連線名稱" prop="name">
          <el-input v-model="form.name" placeholder="例如: 正式環境 MySQL" />
        </el-form-item>

        <el-form-item label="資料庫類型" prop="dbType">
          <el-select v-model="form.dbType" placeholder="請選擇資料庫類型" style="width: 100%" @change="handleDbTypeChange">
            <el-option label="MySQL" value="MySQL" />
            <el-option label="PostgreSQL" value="PostgreSQL" />
            <el-option label="SQL Server" value="SqlServer" />
            <el-option label="MariaDB" value="MariaDB" />
            <el-option label="SQLite" value="SQLite" />
            <el-option label="Oracle" value="Oracle" />
          </el-select>
        </el-form-item>

        <template v-if="form.dbType !== 'SQLite'">
          <el-row :gutter="12">
            <el-col :span="16">
              <el-form-item label="主機 IP" prop="host">
                <el-input v-model="form.host" placeholder="localhost 或 IP" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Port" prop="port" label-width="60px">
                <el-input-number v-model="form.port" :min="1" :max="65535" style="width: 100%" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="預設資料庫" prop="databaseName">
            <el-input v-model="form.databaseName" placeholder="選填資料庫名稱" />
          </el-form-item>

          <el-form-item label="使用者名稱" prop="username">
            <el-input v-model="form.username" placeholder="root / postgres / sa" />
          </el-form-item>

          <el-form-item label="密碼" prop="password">
            <el-input
              v-model="form.password"
              type="password"
              show-password
              placeholder="請輸入密碼"
            />
          </el-form-item>
        </template>

        <template v-else>
          <el-form-item label="檔案路徑" prop="databaseName">
            <el-input v-model="form.databaseName" placeholder="例如: C:/data/app.db" />
          </el-form-item>
        </template>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <el-button :loading="dtoTesting" @click="handleTestDto">測試連線</el-button>
          <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">
            {{ isEdit ? '儲存變更' : '建立連線' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { Plus, Refresh } from '@element-plus/icons-vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { useConnectionStore } from '../stores/connectionStore'
import type { Connection } from '../types/connection'

const connectionStore = useConnectionStore()

const dialogVisible = ref(false)
const isEdit = ref(false)
const submitting = ref(false)
const testingId = ref<number | null>(null)
const dtoTesting = ref(false)
const currentEditId = ref<number | null>(null)
const formRef = ref<FormInstance>()

const form = reactive<Partial<Connection>>({
  name: '',
  dbType: 'MySQL',
  host: 'localhost',
  port: 3306,
  databaseName: '',
  username: 'root',
  password: ''
})

const rules: FormRules = {
  name: [{ required: true, message: '請輸入連線名稱', trigger: 'blur' }],
  dbType: [{ required: true, message: '請選擇資料庫類型', trigger: 'change' }],
  host: [{ required: true, message: '請輸入主機 IP', trigger: 'blur' }]
}

onMounted(() => {
  connectionStore.fetchConnections()
})

const getDbTypeTagType = (type: string) => {
  switch (type) {
    case 'MySQL': return 'primary'
    case 'PostgreSQL': return 'success'
    case 'SqlServer': return 'warning'
    case 'SQLite': return 'info'
    default: return ''
  }
}

const handleDbTypeChange = (type: string) => {
  switch (type) {
    case 'MySQL': form.port = 3306; form.username = 'root'; break;
    case 'PostgreSQL': form.port = 5432; form.username = 'postgres'; break;
    case 'SqlServer': form.port = 1433; form.username = 'sa'; break;
    case 'MariaDB': form.port = 3306; form.username = 'root'; break;
    case 'Oracle': form.port = 1521; form.username = 'system'; break;
  }
}

const handleOpenAddModal = () => {
  isEdit.value = false
  currentEditId.value = null
  Object.assign(form, {
    name: '',
    dbType: 'MySQL',
    host: 'localhost',
    port: 3306,
    databaseName: '',
    username: 'root',
    password: ''
  })
  dialogVisible.value = true
}

const handleOpenEditModal = (row: Connection) => {
  isEdit.value = true
  currentEditId.value = row.id
  Object.assign(form, {
    name: row.name,
    dbType: row.dbType,
    host: row.host,
    port: row.port,
    databaseName: row.databaseName,
    username: row.username,
    password: row.password
  })
  dialogVisible.value = true
}

const handleTestConnection = async (id: number) => {
  testingId.value = id
  try {
    const res = await connectionStore.testConnection(id)
    if (res && res.success) {
      ElMessage.success('連線測試成功！')
    } else {
      ElMessage.error(res?.message || '連線測試失敗！')
    }
  } catch (err: any) {
    ElMessage.error('連線測試失敗：' + (err.message || '無法連線至伺服器'))
  } finally {
    testingId.value = null
  }
}

const handleTestDto = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    dtoTesting.value = true
    try {
      const res = await connectionStore.testConnectionDto(form)
      if (res && res.success) {
        ElMessage.success('連線測試成功！')
      } else {
        ElMessage.error(res?.message || '連線測試失敗！')
      }
    } catch (err: any) {
      ElMessage.error('連線測試失敗：' + (err.message || '無法連線至伺服器'))
    } finally {
      dtoTesting.value = false
    }
  })
}

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    submitting.value = true
    try {
      if (isEdit.value && currentEditId.value) {
        const res = await connectionStore.updateConnection(currentEditId.value, form)
        if (res && res.success) {
          ElMessage.success('連線設定已更新')
          dialogVisible.value = false
        }
      } else {
        const res = await connectionStore.createConnection(form)
        if (res && res.success) {
          ElMessage.success('連線設定已建立')
          dialogVisible.value = false
        }
      }
    } finally {
      submitting.value = false
    }
  })
}

const handleDelete = async (id: number) => {
  const res = await connectionStore.deleteConnection(id)
  if (res && res.success) {
    ElMessage.success('已刪除連線設定')
  }
}
</script>

<style scoped lang="scss">
.page-container {
  padding: 24px;

  .toolbar {
    margin-bottom: 16px;
    display: flex;
    gap: 12px;
  }

  .table-card {
    background-color: #1d263b;
    border-color: #2e3a52;
  }

  .dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }
}
</style>
