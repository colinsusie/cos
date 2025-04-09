// Written by Colin on 2024-1-10

using CoLib.Container;

namespace CoLib.Entities;

/// <summary>
/// 创建实体的工厂
/// </summary>
public static class EntityFactory
{
    public static TEntity Create<TEntity, TBuilder>(DataSet dataSet) 
        where TEntity: Entity 
        where TBuilder: IEntityBuilder<TEntity>
    {
        var entity = TBuilder.CreateEntity(dataSet);
        var components = TBuilder.CreateComponents(dataSet);
        entity.InitComponents(components, dataSet);
        entity.InitCompleted(dataSet);
        return entity;
    }
}

/// <summary>
/// 实体构建器
/// </summary>
public interface IEntityBuilder<out T>
{
    /// <summary>
    /// 创建实体类 
    /// </summary>
    public static abstract T CreateEntity(DataSet dataSet);
    /// <summary>
    /// 创建组件列表 
    /// </summary>
    public static abstract List<Component>? CreateComponents(DataSet dataSet);
}