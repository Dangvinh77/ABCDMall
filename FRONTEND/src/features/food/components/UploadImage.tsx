export default function UploadImage({ onChange }: any) {
  const handleFile = (e: any) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
      onChange(reader.result); // base64
    };
    reader.readAsDataURL(file);
  };

  return (
    <input
      type="file"
      accept="image/*"
      onChange={handleFile}
      className="mt-2 text-sm"
    />
  );
}