namespace Platform::Memory
{
    template <typename TSelf, typename... TBase>
    class Polymorph : public TBase...
    {
    public:
        // Optional static polymorphism - allows getting the derived type
        TSelf& self() { return static_cast<TSelf&>(*this); }
        const TSelf& self() const { return static_cast<const TSelf&>(*this); }
        
        // Enable perfect forwarding to derived class methods
        template<typename T = TSelf>
        T& as() { return static_cast<T&>(*this); }
        
        template<typename T = TSelf>
        const T& as() const { return static_cast<const T&>(*this); }
    };
}