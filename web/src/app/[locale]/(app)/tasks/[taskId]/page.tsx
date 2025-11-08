import { TaskDetailsView } from "@/features/tasks/components/TaskDetailsView";

interface TaskDetailsPageProps {
  params: {
    taskId: string;
  };
}

export default function TaskDetailsPage({ params }: TaskDetailsPageProps) {
  return <TaskDetailsView taskId={params.taskId} />;
}

