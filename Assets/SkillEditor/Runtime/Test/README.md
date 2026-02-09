# GAS 技能系统测试指南

## 测试文件说明

| 文件 | 说明 |
|------|------|
| `TestAbilityData.cs` | 测试用技能数据工厂，创建预设的技能图表 |
| `TestAttributeSetup.cs` | 属性设置工具，为ASC配置基础属性 |
| `GASTestRunner.cs` | MonoBehaviour测试组件，支持按键测试 |
| `GASConsoleTest.cs` | 控制台测试，可通过菜单运行 |
| `GASTestUI.cs` | 带UI界面的测试组件 |
| `Editor/GASTestMenu.cs` | Unity编辑器菜单入口 |

## 测试方法

### 方法一：编辑器菜单测试（推荐新手）

1. 打开Unity编辑器
2. 点击菜单 `Tools > GAS > 运行控制台测试`
3. 查看Console窗口的测试结果

### 方法二：场景测试（推荐完整测试）

1. 创建新场景或打开现有场景
2. 点击菜单 `Tools > GAS > 创建测试场景对象`
3. 运行场景（Play）
4. 使用按键测试：
   - `1` - 释放伤害技能（对目标造成50点伤害）
   - `2` - 释放治疗技能（恢复自身30点生命）
   - `3` - 释放概率技能（50%概率暴击）
   - `R` - 重置测试
   - `P` - 打印当前状态

### 方法三：UI界面测试

1. 创建空GameObject
2. 添加 `GASTestUI` 组件
3. 运行场景
4. 使用屏幕左上角的UI按钮进行测试

## 预设技能说明

### 1. 简单伤害技能
```
流程: Ability -> WaitDelay(0.5s) -> Damage(50) -> EndAbility
效果: 等待0.5秒后对目标造成50点物理伤害
```

### 2. 简单治疗技能
```
流程: Ability -> Heal(30) -> EndAbility
效果: 立即恢复施法者30点生命值
```

### 3. 概率技能
```
流程: Ability -> Probability(50%)
              ├─[成功]-> Damage(100) -> EndAbility
              └─[失败]-> Damage(30) -> EndAbility
效果: 50%概率造成100点伤害（暴击），否则造成30点伤害
```

## 预期测试结果

✓ ASC创建测试通过
✓ 属性系统测试通过
✓ 技能授予测试通过
✓ 伤害技能测试通过
✓ 治疗技能测试通过

## 常见问题

### Q: 测试报错 "未找到属性"
A: 确保调用了 `TestAttributeSetup.CreateBasicAttributes()` 创建属性

### Q: 技能激活失败
A: 检查技能是否已授予，以及是否满足激活条件（标签检查）

### Q: 伤害/治疗数值不对
A: 检查防御属性设置，伤害会经过护甲减免计算
