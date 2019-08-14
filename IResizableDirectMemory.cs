namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a resizable memory block interface with direct access (via unmanaged pointers).</para>
    /// <para>Представляет интерфейс блока памяти c изменяемым размером и прямым доступом (через неуправляемые указатели).</para>
    /// </summary>
    /// <remarks>
    /// Возможно, если дать настройку - инициализировать ли нулями выделяемую память можно немного выиграть в производительности,
    /// однако потерять в надёжности и стабильности при разработке.
    /// Возможно ReservedCapacity - лишнее свойство, и управлять размером блока можно через что-то одно.
    /// Либо может быть ReservedCapacity можно сделать readonly.
    /// Возможно нужна настройка, определяющая блоками какого размера растёт блок памяти.
    /// Можно ли как-то отлавливать ошибки при обращении к некорректному указателю и
    /// автоматически трактовать это как команду к расширению блока?
    /// 
    /// Можно ли реализовать распределённую память, храняющуюся на нескольких машинах, используя такой интерфейс?
    /// 
    /// Асинхронный доступ к памяти? (Для операций выделения памяти, обращения к памяти, изменения размера блока)
    /// 
    /// Нехранимая память? Лог?
    /// 
    /// Возможно потребуется добавить событие OnPointerChanged/OnSizeChanged.
    /// </remarks>
    public interface IResizableDirectMemory : IDirectMemory
    {
        /// <summary>
        /// <para>Gets or sets the reserved capacity in bytes of this memory block.</para>
        /// <para>Возвращает или устаналивает зарезервированный размер блока памяти в байтах.</para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// If less then zero the value is replaced with zero.
        /// Cannot be less than the used capacity of this memory block.
        /// </para>
        /// <para>
        /// Если меньше нуля, значение заменяется на ноль.
        /// Не может быть меньше используемой емкости блока памяти.
        /// </para>
        /// </remarks>
        long ReservedCapacity { get; set; }

        /// <summary>
        /// <para>Gets or sets the used capacity in bytes of this memory block.</para>
        /// <para>Возвращает или устанавливает используемый размер в блоке памяти (в байтах).</para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// If less then zero the value is replaced with zero.
        /// Cannot be greater than the reserved capacity of this memory block.
        /// </para>
        /// <para>
        /// It is recommended to reduce the reserved capacity of the memory block to the used capacity (specified in this property) after the completion of the use of the memory block.
        /// </para>
        /// <para>
        /// Если меньше нуля, значение заменяется на ноль.
        /// Не может быть больше, чем зарезервированная емкость этого блока памяти.
        /// </para>
        /// <para>
        /// Рекомендуется уменьшать фактический размер блока памяти до используемого размера (указанного в этом свойстве) после завершения использования блока памяти.
        /// </para>
        /// </remarks>
        long UsedCapacity { get; set; }
    }
}